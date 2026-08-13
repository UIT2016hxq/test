namespace eft_dma_radar.Common.DMA
{
    /// <summary>
    /// Process-wide safety policy for this build. The radar may read game memory,
    /// but it must never alter the target process.
    /// </summary>
    internal static class MemoryWritePolicy
    {
        /// <summary>
        /// Kept as a compile-time policy so an imported or tampered config cannot
        /// re-enable writes at runtime.
        /// </summary>
        public const bool IsWriteAllowed = false;

        public static void EnsureWriteAllowed()
        {
            if (!IsWriteAllowed)
                throw new InvalidOperationException(
                    "Memory writes are disabled in this read-only radar build.");
        }
    }
}
