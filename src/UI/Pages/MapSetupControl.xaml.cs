using eft_dma_radar.Tarkov.EFTPlayer;
using eft_dma_radar.UI.Misc;
using eft_dma_radar.Common.Maps;
using eft_dma_radar.Common.Misc;
using HandyControl.Controls;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Numerics;
using Point = System.Windows.Point;
using UserControl = System.Windows.Controls.UserControl;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using NumericUpDown = HandyControl.Controls.NumericUpDown;

namespace eft_dma_radar.UI.Pages
{
    /// <summary>
    /// Interaction logic for MapSetupControl.xaml
    /// </summary>
    public partial class MapSetupControl : UserControl
    {
        #region Fields and Properties
        private Point _dragStartPoint;
        public event EventHandler CloseRequested;
        public event EventHandler BringToFrontRequested;
        public event EventHandler<PanelDragEventArgs> DragRequested;
        public event EventHandler? SaveCalibrationRequested;
        public event EventHandler? ResetCalibrationRequested;
        public event EventHandler<MapFloorOverrideChangedEventArgs>? FloorOverrideChanged;
        public bool IsCapturingAnchors { get; private set; }
        private (Vector2 World, Vector2 Map)? _firstAnchor;
        private bool _updating;
        #endregion

        public MapSetupControl()
        {
            InitializeComponent();

            nudMapX.ValueChanged += MapSetupControl_ValueChanged;
            nudMapY.ValueChanged += MapSetupControl_ValueChanged;
            nudMapScale.ValueChanged += MapSetupControl_ValueChanged;
        }

        #region Functions
        /// <summary>
        /// Updates the player position display with current coordinates
        /// </summary>
        public void UpdatePlayerPosition(LocalPlayer player)
        {
            var pos = player.Position;
            txtPlayerX.Text = pos.X.ToString("0.000");
            txtPlayerY.Text = pos.Z.ToString("0.000"); // Z & Y Swapped cus of EFT gg
            txtPlayerZ.Text = pos.Y.ToString("0.000");
        }

        /// <summary>
        /// Updates the map configuration fields with current values
        /// </summary>
        public void UpdateMapConfiguration(float x, float y, float scale, int layerCount = 1, int? floorOverride = null)
        {
            _updating = true;
            nudMapX.Value = (double)x;
            nudMapY.Value = (double)y;
            nudMapScale.Value = (double)scale;
            nudFloor.Maximum = Math.Max(0, layerCount - 1);
            chkAutoFloor.IsChecked = floorOverride is null;
            nudFloor.Value = floorOverride ?? 0;
            nudFloor.IsEnabled = floorOverride is not null && layerCount > 1;
            _updating = false;
        }

        public bool CaptureAnchor(Vector2 world, Vector2 map, float svgScale, out float x, out float y, out float scale)
        {
            x = y = 0;
            scale = 1;
            if (!IsCapturingAnchors)
                return false;

            if (_firstAnchor is null)
            {
                _firstAnchor = (world, map);
                txtCalibrationStatus.Text = "已记录锚点 1。移动到第二个已知地标后，在地图上单击该地标。";
                return false;
            }

            var first = _firstAnchor.Value;
            var deltaWorld = world - first.World;
            var deltaMap = map - first.Map;
            if (Math.Abs(deltaWorld.X) < 0.1f || Math.Abs(deltaWorld.Y) < 0.1f || svgScale <= 0)
            {
                txtCalibrationStatus.Text = "两个锚点距离过近或位于同一轴线；请选择相距更远的地标。";
                return false;
            }

            var scaleX = deltaMap.X / (deltaWorld.X * svgScale);
            var scaleZ = -deltaMap.Y / (deltaWorld.Y * svgScale);
            if (scaleX <= 0 || scaleZ <= 0 || Math.Abs(scaleX - scaleZ) > Math.Max(scaleX, scaleZ) * 0.15f)
            {
                txtCalibrationStatus.Text = "两点不匹配：请确认点击的是地图上的真实地标后重试。";
                _firstAnchor = null;
                return false;
            }

            scale = (scaleX + scaleZ) * 0.5f;
            x = (first.Map.X / svgScale) - (first.World.X * scale);
            y = (first.Map.Y / svgScale) + (first.World.Y * scale);
            IsCapturingAnchors = false;
            _firstAnchor = null;
            btnCancelCalibration.Visibility = Visibility.Collapsed;
            btnTwoPointCalibration.IsEnabled = true;
            txtCalibrationStatus.Text = "两点校准完成；请确认当前位置后点击“保存此地图校准”。";
            return true;
        }
        #endregion

        #region Event Handlers
        private void btnCloseHeader_Click(object sender, RoutedEventArgs e)
        {
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }

        private void MapSetupControl_ValueChanged(object sender, HandyControl.Data.FunctionEventArgs<double> e)
        {
            if (_updating || !Memory.InRaid || Memory.LocalPlayer is null)
                return;

            if (sender is NumericUpDown nud && nud.Tag is string tag)
            {
                var value = (float)nud.Value;
                var map = XMMapManager.Map.Config;

                switch (tag)
                {
                    case "xOffset":
                        map.X = value;
                        break;
                    case "yOffset":
                        map.Y = value;
                        break;
                    case "Scale":
                        map.Scale = value;
                        break;
                }
            }
        }

        private void btnSaveCalibration_Click(object sender, RoutedEventArgs e) => SaveCalibrationRequested?.Invoke(this, EventArgs.Empty);

        private void btnResetCalibration_Click(object sender, RoutedEventArgs e) => ResetCalibrationRequested?.Invoke(this, EventArgs.Empty);

        private void btnTwoPointCalibration_Click(object sender, RoutedEventArgs e)
        {
            IsCapturingAnchors = true;
            _firstAnchor = null;
            btnTwoPointCalibration.IsEnabled = false;
            btnCancelCalibration.Visibility = Visibility.Visible;
            txtCalibrationStatus.Text = "锚点 1：在地图上单击你当前所在的已知地标。";
        }

        private void btnCancelCalibration_Click(object sender, RoutedEventArgs e)
        {
            IsCapturingAnchors = false;
            _firstAnchor = null;
            btnTwoPointCalibration.IsEnabled = true;
            btnCancelCalibration.Visibility = Visibility.Collapsed;
            txtCalibrationStatus.Text = "已取消两点校准。";
        }

        private void chkAutoFloor_Changed(object sender, RoutedEventArgs e)
        {
            if (_updating) return;
            var automatic = chkAutoFloor.IsChecked == true;
            nudFloor.IsEnabled = !automatic && nudFloor.Maximum > 0;
            FloorOverrideChanged?.Invoke(this, new MapFloorOverrideChangedEventArgs(automatic ? null : (int?)nudFloor.Value));
        }

        private void nudFloor_ValueChanged(object sender, HandyControl.Data.FunctionEventArgs<double> e)
        {
            if (!_updating && chkAutoFloor.IsChecked != true)
                FloorOverrideChanged?.Invoke(this, new MapFloorOverrideChangedEventArgs((int)nudFloor.Value));
        }
        #endregion

        #region Drag Handling
        private void DragHandle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            BringToFrontRequested?.Invoke(this, EventArgs.Empty);

            DragHandle.CaptureMouse();
            _dragStartPoint = e.GetPosition(this);

            DragHandle.MouseMove += DragHandle_MouseMove;
            DragHandle.MouseLeftButtonUp += DragHandle_MouseLeftButtonUp;
        }

        private void DragHandle_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var currentPosition = e.GetPosition(this);
                var offset = currentPosition - _dragStartPoint;

                DragRequested?.Invoke(this, new PanelDragEventArgs(offset.X, offset.Y));
            }
        }

        private void DragHandle_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            DragHandle.ReleaseMouseCapture();
            DragHandle.MouseMove -= DragHandle_MouseMove;
            DragHandle.MouseLeftButtonUp -= DragHandle_MouseLeftButtonUp;
        }
        #endregion
    }

    public sealed class MapFloorOverrideChangedEventArgs : EventArgs
    {
        public MapFloorOverrideChangedEventArgs(int? floor) => Floor = floor;
        public int? Floor { get; }
    }
}
