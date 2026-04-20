<script>
  import { onMount, afterUpdate, tick } from 'svelte'
  import { sendClose } from '../lib/messaging.js'

  export let logs = []
  export let onClose = () => {}

  const MAX_LOGS = 2000
  const ROW_HEIGHT = 28

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

  // Virtualized rendering
  let scrollTop = 0
  let containerHeight = 0

  $: totalHeight = filteredLogs.length * ROW_HEIGHT
  $: startIndex = Math.max(0, Math.floor(scrollTop / ROW_HEIGHT) - 5)
  $: endIndex = Math.min(filteredLogs.length, Math.ceil((scrollTop + containerHeight) / ROW_HEIGHT) + 5)
  $: visibleLogs = filteredLogs.slice(startIndex, endIndex)
  $: offsetY = startIndex * ROW_HEIGHT

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
    class="flex-1 overflow-y-auto font-mono text-xs leading-none"
  >
    <div style="height: {totalHeight}px; position: relative;">
      <div style="transform: translateY({offsetY}px);">
        {#each visibleLogs as log, i (startIndex + i)}
          <div
            class="flex items-baseline gap-2 px-3 hover:bg-gray-800/60 border-b border-gray-800"
            style="height: {ROW_HEIGHT}px;"
          >
            <span class="text-gray-500 shrink-0 tabular-nums w-[72px]">{log.timestamp}</span>
            <span class="shrink-0 w-[28px] font-bold {levelColor(log.level)}">{levelBadge(log.level)}</span>
            <span class="text-gray-500 shrink-0 max-w-[200px] truncate" title={log.category}>
              {log.category.split('.').pop()}
            </span>
            <span class="text-gray-200 truncate flex-1" title={log.message}>{log.message}</span>
          </div>
        {/each}
      </div>
    </div>
  </div>
</div>
