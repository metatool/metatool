<script>
  import { onMount, afterUpdate } from 'svelte'
  import { virtualizeList } from '../lib/logListVirtualize.js'

  export let logs = []
  export let onClose = () => {}

  const MAX_LOGS = 2000
  const MIN_CATEGORY_COL_CH = 10
  const MAX_CATEGORY_COL_CH = 18

  let filterLevel = 'all'
  let searchQuery = ''
  let autoScroll = true
  let containerEl
  let inputEl

  $: filteredLogs = logs.filter(log => {
    if (filterLevel !== 'all' && log.level !== filterLevel) return false
    if (searchQuery.trim()) {
      const q = searchQuery.toLowerCase()
      return log.message.toLowerCase().includes(q) ||
             log.category.toLowerCase().includes(q)
    }
    return true
  })

  let scrollTop = 0
  let containerHeight = 0

  $: ({
    rowHeights,
    totalHeight,
    startIndex,
    visibleItems: visibleLogs,
    offsetY,
    singleRowHeight,
  } = virtualizeList(filteredLogs, scrollTop, containerHeight))
  $: maxCategoryChars = filteredLogs.reduce((max, log) => {
    const categoryName = (log.category || '').split('.').pop() || ''
    return Math.max(max, categoryName.length)
  }, 0)
  $: categoryColCh = Math.min(MAX_CATEGORY_COL_CH, Math.max(MIN_CATEGORY_COL_CH, maxCategoryChars + 1))

  function handleScroll() {
    if (!containerEl) return
    scrollTop = containerEl.scrollTop
    containerHeight = containerEl.clientHeight

    const atBottom = containerEl.scrollTop + containerEl.clientHeight >= containerEl.scrollHeight - 40
    autoScroll = atBottom
  }

  afterUpdate(() => {
    if (autoScroll && containerEl) {
      containerEl.scrollTop = containerEl.scrollHeight
    }
  })

  function onKey(e) {
    if (e.key === 'Escape') {
      onClose()
    }
  }

  function clearLogs() {
    logs = []
  }

  function scrollToBottom() {
    autoScroll = true
    if (containerEl) {
      containerEl.scrollTop = containerEl.scrollHeight
    }
  }

  function levelColor(level) {
    switch (level) {
      case 'Critical': return 'text-white bg-red-700'
      case 'Error': return 'text-red-600'
      case 'Warning': return 'text-yellow-600'
      case 'Information': return 'text-blue-600'
      case 'Debug': return 'text-gray-500'
      case 'Trace': return 'text-gray-400'
      default: return 'text-gray-600'
    }
  }

  function levelMsgColor(level) {
    switch (level) {
      case 'Critical': return 'text-white bg-red-700'
      case 'Error': return 'text-red-600'
      case 'Warning': return 'text-yellow-400'
      case 'Information': return 'text-gray-200'
      case 'Debug': return 'text-gray-400'
      case 'Trace': return 'text-gray-400'
      default: return 'text-gray-200'
    }
  }

  function levelBadge(level) {
    switch (level) {
      case 'Critical': return 'CRT'
      case 'Error': return 'ERR'
      case 'Warning': return 'WRN'
      case 'Information': return 'INF'
      case 'Debug': return 'DBG'
      case 'Trace': return 'TRC'
      default: return level?.substring(0, 3) || '???'
    }
  }

  onMount(() => {
    if (containerEl) {
      containerHeight = containerEl.clientHeight
    }
    if (inputEl) inputEl.focus()
  })
</script>

<svelte:window on:keydown={onKey} />
<div class="flex flex-col w-full h-full bg-gray-900 text-gray-200 rounded-lg overflow-hidden" role="log">
  <!-- Toolbar -->
  <div class="flex items-center gap-2 px-3 py-2 bg-gray-800 border-b border-gray-700 shrink-0">
    <input
      bind:this={inputEl}
      bind:value={searchQuery}
      class="flex-1 px-3 py-1.5 text-sm bg-gray-700 text-gray-200 rounded border border-gray-600
             focus:outline-none focus:ring-1 focus:ring-blue-500 placeholder-gray-400"
      placeholder="Filter logs..."
      autocomplete="off"
    />

    <select
      bind:value={filterLevel}
      class="px-2 py-1.5 text-sm bg-gray-700 text-gray-200 rounded border border-gray-600
             focus:outline-none focus:ring-1 focus:ring-blue-500"
    >
      <option value="all">All Levels</option>
      <option value="Trace">Trace</option>
      <option value="Debug">Debug</option>
      <option value="Information">Info</option>
      <option value="Warning">Warning</option>
      <option value="Error">Error</option>
      <option value="Critical">Critical</option>
    </select>

    <span class="text-xs text-gray-400 tabular-nums whitespace-nowrap">
      {filteredLogs.length}/{logs.length}
    </span>

    <button
      on:click={scrollToBottom}
      class="p-1.5 rounded hover:bg-gray-600 text-gray-400 hover:text-gray-200 transition-colors"
      class:text-blue-400={autoScroll}
      title="Auto-scroll to bottom"
    >
      <svg xmlns="http://www.w3.org/2000/svg" class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
        <path stroke-linecap="round" stroke-linejoin="round" d="M19 14l-7 7m0 0l-7-7m7 7V3" />
      </svg>
    </button>

    <button
      on:click={clearLogs}
      class="p-1.5 rounded hover:bg-gray-600 text-gray-400 hover:text-gray-200 transition-colors"
      title="Clear logs"
    >
      <svg xmlns="http://www.w3.org/2000/svg" class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
        <path stroke-linecap="round" stroke-linejoin="round" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
      </svg>
    </button>

    <!-- <button
      on:click={onClose}
      class="p-1.5 rounded hover:bg-gray-600 text-gray-400 hover:text-gray-200 transition-colors"
      title="Close (Esc)"
    >
      <svg xmlns="http://www.w3.org/2000/svg" class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
        <path stroke-linecap="round" stroke-linejoin="round" d="M6 18L18 6M6 6l12 12" />
      </svg>
    </button> -->
  </div>

  <!-- Log entries (virtualized) -->
  <div
    bind:this={containerEl}
    on:scroll={handleScroll}
    class="dark-scrollbar flex-1 overflow-y-auto font-mono text-xs leading-none"
  >
    <div style="height: {totalHeight}px; position: relative;">
      <div style="transform: translateY({offsetY}px);">
        {#each visibleLogs as log, i (startIndex + i)}
          {@const h = rowHeights[startIndex + i]}
          {@const multi = h > singleRowHeight}
          <div
            class="grid gap-2 px-3 hover:bg-gray-800/60 border-b border-gray-800 {multi ? 'items-start py-1.5' : 'items-center'}"
            style="height: {h}px; grid-template-columns: 74px 36px {categoryColCh}ch minmax(0, 1fr);"
          >
            <span class="text-gray-500 tabular-nums">{log.timestamp}</span>
            <span class="text-center font-bold {levelColor(log.level)}">{levelBadge(log.level)}</span>
            <span class="text-gray-500 truncate" title={log.category}>
              {log.category.split('.').pop()}
            </span>
            <span
              class="text-gray-200 truncate min-w-0 overflow-hidden whitespace-pre tab-size-4 {levelMsgColor(log.level)} {multi ? 'leading-4' : 'text-ellipsis'}"
              title={log.message}
            >{log.message}</span>
          </div>
        {/each}
      </div>
    </div>
  </div>
</div>
