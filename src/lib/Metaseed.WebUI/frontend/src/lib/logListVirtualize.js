/**
 * Variable-height list virtualization (prefix offsets + binary search).
 * Row height is derived from newline count in `item.message`.
 */

export const LOG_LIST_VIRTUAL_DEFAULTS = {
  rowHeight: 28,
  lineHeight: 16,
  rowVpad: 12,
  overscanBefore: 5,
  overscanAfter: 6,
}

export function lineCount(msg) {
  if (!msg) return 1
  let n = 1
  for (let i = 0; i < msg.length; i++) if (msg.charCodeAt(i) === 10) n++
  return n
}

export function rowHeightForItem(item, opts = LOG_LIST_VIRTUAL_DEFAULTS) {
  const lines = lineCount(item?.message)
  if (lines <= 1) return opts.rowHeight
  return lines * opts.lineHeight + opts.rowVpad
}

export function buildRowOffsets(rowHeights) {
  const arr = new Array(rowHeights.length + 1)
  arr[0] = 0
  for (let i = 0; i < rowHeights.length; i++) arr[i + 1] = arr[i] + rowHeights[i]
  return arr
}

/** Smallest i such that offsets[i + 1] > target */
export function findRowAtOffset(offsets, target) {
  let lo = 0
  let hi = offsets.length - 1
  while (lo < hi) {
    const mid = (lo + hi) >> 1
    if (offsets[mid + 1] <= target) lo = mid + 1
    else hi = mid
  }
  return lo
}

/**
 * @param {unknown[]} items
 * @param {number} scrollTop
 * @param {number} containerHeight
 * @param {object} [options] optional overrides for LOG_LIST_VIRTUAL_DEFAULTS fields
 */
export function virtualizeList(items, scrollTop, containerHeight, options = {}) {
  const opts = { ...LOG_LIST_VIRTUAL_DEFAULTS, ...options }
  const rowHeights = items.map((item) => rowHeightForItem(item, opts))
  const rowOffsets = buildRowOffsets(rowHeights)
  const n = items.length
  const totalHeight = rowOffsets[n] || 0

  let startIndex = 0
  let endIndex = 0
  if (n > 0) {
    startIndex = Math.max(0, findRowAtOffset(rowOffsets, scrollTop) - opts.overscanBefore)
    endIndex = Math.min(n, findRowAtOffset(rowOffsets, scrollTop + containerHeight) + opts.overscanAfter)
  }

  return {
    rowHeights,
    rowOffsets,
    totalHeight,
    startIndex,
    endIndex,
    visibleItems: items.slice(startIndex, endIndex),
    offsetY: rowOffsets[startIndex] || 0,
    singleRowHeight: opts.rowHeight,
  }
}
