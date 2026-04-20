<script>
  import { onMount } from 'svelte'
  import HotkeysView from './HotkeysView.svelte'
  import { sendHotkeySelected } from '../lib/messaging.js'

  export let hotkeys = []
  export let onClose = () => {}

  let query = ''
  let selectedIndex = 0
  let inputEl
  let hotkeysViewEl

  $: filteredHotkeys = query.trim()
    ? hotkeys.filter(item =>
      item.hotkey.toLowerCase().includes(query.toLowerCase()) ||
      item.description.toLowerCase().includes(query.toLowerCase())
    )
    : hotkeys

  function handleInput(e) {
    query = e.target.value
    selectedIndex = 0
  }

  function submitSelection() {
    if (selectedIndex >= 0 && filteredHotkeys[selectedIndex]) {
      const item = filteredHotkeys[selectedIndex]
      sendHotkeySelected(item, selectedIndex)
      query = ''
      selectedIndex = 0
    }
  }

  function onKey(e) {
    if (e.key === 'Escape') {
      onClose()
    } else if (e.key === 'Enter') {
      submitSelection()
    } else if (e.key === 'ArrowDown') {
      e.preventDefault()
      selectedIndex = Math.min(selectedIndex + 1, filteredHotkeys.length - 1)
    } else if (e.key === 'ArrowUp') {
      e.preventDefault()
      selectedIndex = Math.max(selectedIndex - 1, 0)
    } else if (e.key === 'Home') {
      e.preventDefault()
      selectedIndex = 0
    } else if (e.key === 'End') {
      e.preventDefault()
      selectedIndex = filteredHotkeys.length - 1
    } else if (e.key === 'PageDown') {
      e.preventDefault()
      const pageSize = hotkeysViewEl?.getPageSize?.() ?? 5
      selectedIndex = Math.min(selectedIndex + pageSize, filteredHotkeys.length - 1)
    } else if (e.key === 'PageUp') {
      e.preventDefault()
      const pageSize = hotkeysViewEl?.getPageSize?.() ?? 5
      selectedIndex = Math.max(selectedIndex - pageSize, 0)
    }
  }

  function handleItemActivate(index) {
    selectedIndex = index
    submitSelection()
  }

  function focusHandler() {
    if (inputEl) inputEl.focus()
  }

  onMount(() => {
    const listener = () => focusHandler()
    document.addEventListener('focus-search', listener)
    setTimeout(focusHandler, 30)
    return () => document.removeEventListener('focus-search', listener)
  })
</script>

<div class="h-full min-h-0 flex-1 overflow-hidden flex flex-col">
  <div class="bg-white flex items-center gap-3 shrink-0">
    <input
      bind:this={inputEl}
      bind:value={query}
      on:input={handleInput}
      on:keydown={onKey}
      on:blur={() => inputEl?.focus()}
      class="flex-1 px-3 py-1.5 text-sm mx-4 rounded-md border border-gray-600 focus:outline-none focus:ring-2 focus:ring-indigo-500"
      placeholder="Search hotkeys... (use ↑↓ to navigate, Enter to execute)"
      autocomplete="off"
    />
  </div>

  <div class="flex-1 min-h-0 overflow-hidden flex flex-col">
    <HotkeysView
      bind:this={hotkeysViewEl}
      {filteredHotkeys}
      {query}
      {selectedIndex}
      onItemActivate={handleItemActivate}
    />
  </div>
</div>
