using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using HandyControl.Controls;

namespace eft_dma_radar.UI.Misc;

/// <summary>
/// Applies Chinese display text without changing control names, tags, serialized
/// settings, or ComboBox values that may be used by feature logic.
/// </summary>
internal static class ChineseUi
{
    private static bool _initialized;

    private static readonly IReadOnlyDictionary<string, string> Text =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["Settings"] = "设置",
            ["General"] = "常规",
            ["General Settings"] = "常规设置",
            ["General Options"] = "常规选项",
            ["Options"] = "选项",
            ["Information"] = "信息",
            ["Import"] = "导入",
            ["Export"] = "导出",
            ["Import/Export Options"] = "导入/导出选项",
            ["Import from Clipboard"] = "从剪贴板导入",
            ["Export to Clipboard"] = "导出到剪贴板",
            ["Show Map Setup"] = "显示地图校准",
            ["Radar FPS Limit"] = "雷达帧率上限",
            ["UI Scale"] = "界面缩放",
            ["Widgets"] = "组件",
            ["Radar Options"] = "雷达选项",
            ["Player Information"] = "玩家信息",
            ["Player Type"] = "玩家类型",
            ["Height Indicator"] = "高度指示器",
            ["Important Indicator"] = "重要目标指示器",
            ["High Alert"] = "高危警报",
            ["Show Important Loot"] = "显示重要战利品",
            ["Render Distance"] = "渲染距离",
            ["Aimline Length"] = "瞄准线长度",
            ["Minimum KD"] = "最低 KD",
            ["Entity Information"] = "实体信息",
            ["Entity Type"] = "实体类型",
            ["Show Radius"] = "显示范围",
            ["Show Locked"] = "显示已锁定",
            ["Show Unlocked"] = "显示未锁定",
            ["Hide Inactive"] = "隐藏未激活",
            ["Show Tripwire Line"] = "显示绊雷连线",
            ["Monitor Settings"] = "显示器设置",
            ["Monitor"] = "显示器",
            ["Width"] = "宽度",
            ["Height"] = "高度",
            ["Refresh Monitors"] = "刷新显示器",
            ["Quest Helper"] = "任务助手",
            ["Enabled"] = "启用",
            ["Select All"] = "全选",
            ["Kappa Filter"] = "Kappa 筛选",
            ["Optional Tasks"] = "可选任务",
            ["Kill Zones"] = "击杀区域",
            ["Web Radar Server"] = "网页雷达服务",
            ["Start"] = "启动",
            ["Stop"] = "停止",
            ["Stopping..."] = "正在停止...",
            ["Starting..."] = "正在启动...",
            ["Port"] = "端口",
            ["Player API Service"] = "玩家资料服务",
            ["Create API File?"] = "创建 API 密钥文件",
            ["Edit API File?"] = "编辑 API 密钥文件",
            ["Open Folder"] = "打开文件夹",
            ["Clear"] = "清除",
            ["No API key saved."] = "未保存 API 密钥。",
            ["API key loaded successfully"] = "API 密钥已成功加载",
            ["Colors"] = "颜色",
            ["Player Colors"] = "玩家颜色",
            ["Loot Colors"] = "战利品颜色",
            ["Other Colors"] = "其他颜色",
            ["Interface Colors"] = "界面颜色",
            ["Fuser HUD Colors"] = "叠加 HUD 颜色",
            ["LocalPlayer"] = "本地玩家",
            ["Friendly"] = "队友",
            ["Player Scav"] = "玩家 Scav",
            ["Focused"] = "当前焦点",
            ["Streamer"] = "主播",
            ["Special Player"] = "特殊玩家",
            ["Raider"] = "精英 AI",
            ["Boss"] = "首领",
            ["Scav"] = "Scav",
            ["Aimbot Target"] = "自瞄目标",
            ["Visible Color"] = "可见颜色",
            ["Regular Loot"] = "普通战利品",
            ["Valuable Loot"] = "高价值战利品",
            ["Wishlist Loot"] = "愿望单战利品",
            ["Containers"] = "容器",
            ["Meds Filter"] = "医疗物品筛选",
            ["Food Filter"] = "食物筛选",
            ["Backpacks Filter"] = "背包筛选",
            ["Quest Loot"] = "任务战利品",
            ["Weapons Filter"] = "武器筛选",
            ["Quest Items/Zones"] = "任务物品/区域",
            ["Airdrops"] = "空投",
            ["Death Marker"] = "死亡标记",
            ["Corpse"] = "尸体",
            ["Explosives"] = "爆炸物",
            ["Switches"] = "开关",
            ["Quest Kill Zones"] = "任务击杀区域",
            ["Exfil Open"] = "撤离点开放",
            ["Exfil Pending"] = "撤离点等待中",
            ["Exfil Closed"] = "撤离点关闭",
            ["Exfil Inactive"] = "撤离点未激活",
            ["Transit"] = "转场点",
            ["Door Open"] = "门已打开",
            ["Door Locked"] = "门已锁定",
            ["Door Shut"] = "门已关闭",
            ["Group Lines"] = "队伍连线",
            ["Raid Stats"] = "对局统计",
            ["Status Text"] = "状态文本",
            ["Magazine Info"] = "弹匣信息",
            ["Energy Bar"] = "能量条",
            ["Hydration Bar"] = "水分条",
            ["Crosshair"] = "准星",
            ["ESP Settings"] = "ESP 设置",
            ["Chams"] = "透视材质",
            ["Fuser"] = "屏幕叠加",
            ["Enable Chams"] = "启用透视材质",
            ["Material Management"] = "材质管理",
            ["Materials Status:"] = "材质状态：",
            ["Unknown"] = "未知",
            ["0/0 loaded"] = "已加载 0/0",
            ["? Refresh Materials"] = "刷新材质",
            ["?? Clear Cache"] = "清除缓存",
            ["Force refresh all chams materials. Use if materials failed to load properly."] = "强制刷新全部透视材质。材质未正确加载时可使用。",
            ["Clear chams material cache and force full reload on next refresh."] = "清除透视材质缓存，并在下次刷新时重新完整加载。",
            ["Advanced Material Types"] = "高级材质类型",
            ["Advanced Memory Writes"] = "高级内存写入",
            ["require"] = "需要",
            ["Entity Settings"] = "实体设置",
            ["Entity Chams"] = "实体透视材质",
            ["Material Type"] = "材质类型",
            ["Material Types"] = "材质类型",
            ["Material Color Selection"] = "材质颜色选择",
            ["Clothing Chams"] = "服装透视材质",
            ["Gear Chams"] = "装备透视材质",
            ["Death Material"] = "死亡材质",
            ["Visible"] = "可见",
            ["Invisible Color"] = "不可见颜色",
            ["Start ESP"] = "启动 ESP",
            ["Auto Fullscreen"] = "自动全屏",
            ["Target Monitor"] = "目标显示器",
            ["Select which monitor to display ESP on"] = "选择用于显示 ESP 的显示器",
            ["FPS Cap"] = "帧率上限",
            ["Font Scale"] = "字体缩放",
            ["Line Scale"] = "线条缩放",
            ["Crosshair Settings"] = "准星设置",
            ["Enable Crosshair"] = "启用准星",
            ["Crosshair Scale"] = "准星缩放",
            ["Mini Radar Settings"] = "小地图设置",
            ["Enable Mini Radar"] = "启用小地图",
            ["High Alert Indicator"] = "高危警报指示器",
            ["Override Text Color"] = "覆盖文本颜色",
            ["Render Mode"] = "渲染模式",
            ["Show Trail"] = "显示轨迹",
            ["Trail Duration"] = "轨迹持续时间",
            ["Min Trail Distance"] = "最小轨迹距离",
            ["Basic"] = "基础",
            ["Bones"] = "骨骼",
            ["Box"] = "方框",
            ["Head Dot"] = "头部圆点",
            ["Circle"] = "圆形",
            ["Dot"] = "圆点",
            ["Square"] = "方形",
            ["Diamond"] = "菱形",
            ["Teammate"] = "队友",
            ["Raider/Rogue/Guard"] = "Raider/Rogue/Guard",
            ["AI"] = "AI",
            ["Scale"] = "缩放",
            ["Off"] = "关闭",
            ["Max"] = "最大",
            ["Fireport Aim"] = "枪口瞄准线",
            ["Aimbot FOV"] = "自瞄 FOV",
            ["Aimbot Lock"] = "自瞄锁定",
            ["Closest Player"] = "最近玩家",
            ["Top Loot"] = "最高价值战利品",
            ["Radar Theme"] = "雷达主题",
            ["Override Player Text"] = "覆盖玩家文字颜色",
            ["Primary Accent"] = "主强调色",
            ["Region"] = "区域",
            ["Secondary Region"] = "次级区域",
            ["Border"] = "边框",
            ["Radar Background"] = "雷达背景",
            ["Fuser Background"] = "叠加窗口背景",
            ["Hotkeys"] = "快捷键",
            ["Application Hotkeys"] = "应用快捷键",
            ["Hotkey Configuration"] = "快捷键配置",
            ["Action:"] = "操作：",
            ["Type:"] = "类型：",
            ["Select Key"] = "选择按键",
            ["On Key"] = "按键触发",
            ["Toggle"] = "切换",
            ["Remove Hotkey"] = "删除快捷键",
            ["Add Hotkey"] = "添加快捷键",
            ["Action"] = "操作",
            ["Key"] = "按键",
            ["Type"] = "类型",
            ["Config"] = "配置",
            ["Config Management"] = "配置管理",
            ["Current Config:"] = "当前配置：",
            ["Available Configs:"] = "可用配置：",
            ["Refresh Configs"] = "刷新配置",
            ["New Config Name:"] = "新配置名称：",
            ["Create New"] = "新建",
            ["Delete"] = "删除",
            ["Reset to Default"] = "恢复默认",
            ["Loot"] = "战利品",
            ["Show Loot"] = "显示战利品",
            ["Show Wishlist"] = "显示愿望单",
            ["Price Settings"] = "价格设置",
            ["Item Value Range"] = "物品价值范围",
            ["Regular Below:"] = "普通物品上限：",
            ["Important Above:"] = "重要物品下限：",
            ["Minimum Corpse Value"] = "尸体最低价值",
            ["Price Per Slot"] = "每格价值",
            ["Price Source:"] = "价格来源：",
            ["Flea"] = "跳蚤市场",
            ["Trader"] = "商人",
            ["Quick Filters"] = "快速筛选",
            ["Item Search"] = "物品搜索",
            ["Container Options"] = "容器选项",
            ["Static Containers"] = "固定容器",
            ["Hide Searched"] = "隐藏已搜索",
            ["Loot Filter"] = "战利品筛选",
            ["Export Selected"] = "导出当前筛选",
            ["Export All"] = "导出全部",
            ["Filter Settings"] = "筛选设置",
            ["Static"] = "固定",
            ["Notify"] = "通知",
            ["sec"] = "秒",
            ["Filter Items"] = "筛选物品",
            ["Add"] = "添加",
            ["Remove"] = "移除",
            ["Bulk Actions:"] = "批量操作：",
            ["Color"] = "颜色",
            ["Enable"] = "启用",
            ["Disable"] = "禁用",
            ["Watchlist"] = "观察名单",
            ["Watchlist Management"] = "观察名单管理",
            ["Entry Management"] = "条目管理",
            ["Reason"] = "原因",
            ["Platform"] = "平台",
            ["None"] = "无",
            ["Username"] = "用户名",
            ["Account ID"] = "账号 ID",
            ["Player History"] = "玩家历史",
            ["Name"] = "名称",
            ["Account"] = "账号",
            ["Hours"] = "时长",
            ["Last Seen"] = "最后出现",
            ["Quest Planner"] = "任务规划",
            ["Kappa Required"] = "Kappa 必需",
            ["Hand over items"] = "上交物品",
            ["MISSIONS"] = "任务",
            ["BRING LIST"] = "携带清单",
            ["UNLOCKS"] = "解锁内容",
            ["ALL MAPS"] = "全部地图",
            ["FIND IN RAID"] = "战局内找到",
            ["Map Setup"] = "地图校准",
            ["LocalPlayer Position"] = "本地玩家坐标",
            ["Map Configuration"] = "地图配置",
            ["Search Settings"] = "搜索设置",
            ["Memory Writes"] = "内存写入",
            ["Global Settings"] = "全局设置",
            ["Disable Wepon Collision"] = "禁用武器碰撞",
            ["Remove Attachments"] = "移除配件",
            ["M.U.L.E Mode"] = "M.U.L.E 模式",
            ["Recoil"] = "后坐力",
            ["Sway"] = "晃动",
            ["Head %"] = "头部 %",
            ["Torso %"] = "躯干 %",
            ["Arms %"] = "手臂 %",
            ["Legs %"] = "腿部 %",
            ["Scale:"] = "缩放：",
            ["Distance"] = "距离",
            ["FPS"] = "帧率",
            ["Intensity"] = "强度",
            ["Zoom"] = "缩放",
            ["Mult"] = "倍率",
            ["Hour"] = "小时",
            ["ID"] = "编号",
            ["X:"] = "X 轴：",
            ["Y:"] = "Y 轴：",
            ["Z:"] = "Z 轴：",
            ["No Visor"] = "移除面罩遮挡",
            ["Owl Mode"] = "猫头鹰模式",
            ["Disable Frostbite"] = "禁用冰霜效果",
            ["Disable Head Bobbing"] = "禁用头部晃动",
            ["Disable Inventory Blur"] = "禁用背包模糊",
            ["Instant Plant"] = "快速埋设",
            ["Med Panel"] = "医疗面板",
            ["Hide Raid Code"] = "隐藏对局代码",
            ["Big Heads"] = "大头模式",
            ["Extended Reach"] = "延长交互距离",
            ["Loot Through Wall"] = "隔墙拾取",
            ["Silent Loot"] = "静默拾取",
            ["Configure Big Heads scale"] = "配置大头模式缩放",
            ["Configure Extended Reach"] = "配置交互距离",
            ["Configure FOV settings"] = "配置视野设置",
            ["Configure Long Jump"] = "配置远跳",
            ["Configure Loot Through Wall"] = "配置隔墙拾取",
            ["Configure Move Speed"] = "配置移动速度",
            ["Configure No Recoil"] = "配置无后坐",
            ["Configure Silent Loot"] = "配置静默拾取",
            ["Configure Wide Lean"] = "配置大幅侧身",
            ["Configure brightness"] = "配置亮度",
            ["Configure time of day"] = "配置时间",
            ["Send Stashed DogTags"] = "上传仓库狗牌",
            ["Sends Dogtags from your stash to our API to gather player names and stats. Thank you!"] = "将仓库中的狗牌发送到 API，以收集玩家名称和统计信息。",
            ["Drag filter groups to reorder priority • Drag items to reorder within group • Ctrl+Click or Shift+Click to multi-select items"] = "拖动筛选组可调整优先级；拖动物品可调整组内顺序；Ctrl+单击或 Shift+单击可多选。",
            ["MessageBox"] = "提示",
            ["Master Switch"] = "总开关",
            ["Rage Mode"] = "狂暴模式",
            ["Anti AFK"] = "防挂机",
            ["Test"] = "测试",
            ["Aimbot Settings"] = "自瞄设置",
            ["Mode"] = "模式",
            ["Target Bone"] = "目标部位",
            ["Max Distance"] = "最大距离",
            ["Weapons"] = "武器",
            ["No Malfunctions"] = "无故障",
            ["Mag Drills"] = "快速装填",
            ["Fast Weapon Ops"] = "快速武器操作",
            ["Disable Weapon Collision"] = "禁用武器碰撞",
            ["No Recoil"] = "无后坐",
            ["Movement"] = "移动",
            ["Infinite Stamina"] = "无限耐力",
            ["Fast Duck"] = "快速下蹲",
            ["No Inertia"] = "无惯性",
            ["Wide Lean"] = "大幅侧身",
            ["Long Jump"] = "远跳",
            ["Move Speed"] = "移动速度",
            ["Multiplier"] = "倍率",
            ["World"] = "世界",
            ["Disable Shadows"] = "禁用阴影",
            ["Disable Grass"] = "禁用草丛",
            ["Clear Weather"] = "晴朗天气",
            ["Time of Day"] = "时间",
            ["Full Bright"] = "全亮",
            ["Camera"] = "相机",
            ["Night Vision"] = "夜视",
            ["Thermal Vision"] = "热成像",
            ["Third Person"] = "第三人称",
            ["Misc"] = "杂项",
            ["Cancel"] = "取消",
            ["Yes"] = "是",
            ["No"] = "否",
            ["OK"] = "确定",
            ["Press Any Key"] = "请按任意键",
            ["Text Input"] = "文本输入",
            ["Loading"] = "加载中",
            ["Game Process Not Running!"] = "未检测到游戏进程！",
            ["Debug/test functionality button"] = "调试/测试功能",
            ["Toggle Follow/Free Mode"] = "切换跟随/自由模式",
            ["Opens the loot settings"] = "打开战利品设置",
            ["Opens the loot filter panel"] = "打开战利品筛选面板",
            ["Opens the chams/fuser esp configuration panel"] = "打开 Chams/叠加 ESP 配置面板",
            ["Opens the watchlist panel"] = "打开观察名单面板",
            ["Opens the player history panel"] = "打开玩家历史面板",
            ["Opens the memory writing configuration panel"] = "打开内存写入配置面板",
            ["Opens the general settings panel"] = "打开常规设置面板",
            ["Search settings (Ctrl+F)"] = "搜索设置（Ctrl+F）",
            ["Restarts the Radar for the current raid instance"] = "重启当前对局的雷达",
            ["Opens the quest planner panel"] = "打开任务规划面板",
            ["Double-click on a player to add them to watchlist"] = "双击玩家可将其加入观察名单",
            ["Click to toggle filter group"] = "点击切换筛选组",
            ["Toggle notifications for this item"] = "切换此物品的通知",
            ["Change item color"] = "修改物品颜色",
            ["Toggle item filtering"] = "切换物品筛选",
            ["Add Filter Group"] = "添加筛选组",
            ["Remove Filter Group"] = "移除筛选组",
            ["Interval in seconds for repeat notifications"] = "重复通知的间隔（秒）",
            ["Add Item"] = "添加物品",
            ["Remove Item"] = "移除物品",
            ["Change color of selected items"] = "修改选中物品颜色",
            ["Enable selected items"] = "启用选中物品",
            ["Disable selected items"] = "禁用选中物品",
            ["Toggle notifications for selected items"] = "切换选中物品通知",
            ["Quest Planner Options"] = "任务规划选项",
            ["Quest plan will appear when connected in lobby."] = "在大厅连接后将显示任务规划。",
        };

    public static void Initialize()
    {
        if (_initialized)
            return;

        _initialized = true;
        EventManager.RegisterClassHandler(
            typeof(FrameworkElement),
            FrameworkElement.LoadedEvent,
            new RoutedEventHandler((sender, _) => TranslateElement((FrameworkElement)sender)));
    }

    public static void Apply(DependencyObject root)
    {
        ArgumentNullException.ThrowIfNull(root);
        Visit(root, new HashSet<DependencyObject>());
    }

    private static void Visit(DependencyObject element, ISet<DependencyObject> visited)
    {
        if (!visited.Add(element))
            return;

        TranslateElement(element);

        var childCount = element is Visual || element is System.Windows.Media.Media3D.Visual3D
            ? VisualTreeHelper.GetChildrenCount(element)
            : 0;
        for (var i = 0; i < childCount; i++)
            Visit(VisualTreeHelper.GetChild(element, i), visited);
    }

    private static void TranslateElement(DependencyObject element)
    {
        if (element is Window window)
            window.Title = Translate(window.Title);

        if (element is TextBlock textBlock)
        {
            if (textBlock.Inlines.Count == 0)
                textBlock.Text = Translate(textBlock.Text);
            else
            {
                foreach (var inline in textBlock.Inlines.OfType<Run>())
                    inline.Text = Translate(inline.Text);
            }
        }

        if (element is Run run)
            run.Text = Translate(run.Text);

        if (element is HeaderedContentControl headeredContent)
            headeredContent.Header = TranslateObject(headeredContent.Header);
        else if (element is HeaderedItemsControl headeredItems)
            headeredItems.Header = TranslateObject(headeredItems.Header);

        // Only items with a separate stable Tag are safe to translate. Several
        // untagged items are used directly as saved configuration values.
        if (element is ComboBoxItem comboBoxItem && comboBoxItem.Tag is not null)
            comboBoxItem.Content = TranslateObject(comboBoxItem.Content);
        else if (element is ContentControl contentControl && element is not ComboBoxItem)
            contentControl.Content = TranslateObject(contentControl.Content);

        if (element is FrameworkElement frameworkElement && ToolTipService.GetToolTip(frameworkElement) is string toolTip)
            ToolTipService.SetToolTip(frameworkElement, Translate(toolTip));

        if (element is FrameworkElement titleElement && TitleElement.GetTitle(titleElement) is string title)
            TitleElement.SetTitle(titleElement, Translate(title));

        TranslateStringProperty(element, "MinContent");
        TranslateStringProperty(element, "MaxContent");

        if (element is DataGrid dataGrid)
        {
            foreach (var column in dataGrid.Columns)
                column.Header = TranslateObject(column.Header);
        }
    }

    private static object TranslateObject(object value) => value is string text ? Translate(text) : value;

    private static void TranslateStringProperty(object target, string propertyName)
    {
        var property = target.GetType().GetProperty(propertyName);
        if (property?.CanRead != true || property.CanWrite != true || property.GetValue(target) is not string text)
            return;

        property.SetValue(target, Translate(text));
    }

    private static string Translate(string text) =>
        !string.IsNullOrWhiteSpace(text) && Text.TryGetValue(text, out var translated) ? translated : text;
}
