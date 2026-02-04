<script>
  import { createEventDispatcher, onMount } from 'svelte'
  const dispatch = createEventDispatcher()
  let query = ''
  let inputEl
  let selectedIndex = -1

  export let hotkeys = []
  export let filteredHotkeys = []

  function handleInput(e) {
    query = e.target.value
    dispatch('search', query)
    selectedIndex = -1 // Reset selection when typing
  }

  function selectItem(index) {
    selectedIndex = index
  }

  function submitSelection() {
    if (selectedIndex >= 0 && filteredHotkeys[selectedIndex]) {
      const item = filteredHotkeys[selectedIndex]
      dispatch('selection', item)
      query = ''
      selectedIndex = 0
    }
  }

  function onKey(e) {
    if (e.key === 'Escape') {
      dispatch('close')
    } else if (e.key === 'Enter') {
      submitSelection()
    } else if (e.key === 'ArrowDown') {
      e.preventDefault()
      selectedIndex = Math.min(selectedIndex + 1, filteredHotkeys.length - 1)
    } else if (e.key === 'ArrowUp') {
      e.preventDefault()
      selectedIndex = Math.max(selectedIndex - 1, -1)
    }
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

<div class="bg-white rounded-lg shadow-lg p-4 max-w-2xl mx-auto">
  <div class="flex items-center gap-3 mb-4">
    <input
      bind:this={inputEl}
      bind:value={query}
      on:input={handleInput}
      on:keydown={onKey}
      class="flex-1 px-4 py-3 rounded-md border border-gray-200 focus:outline-none focus:ring-2 focus:ring-indigo-500"
      placeholder="Search hotkeys... (use ↑↓ to navigate, Enter to select)"
      autocomplete="off"
    />
  </div>

  {#if filteredHotkeys.length > 0}
    <div class="border border-gray-200 rounded-md max-h-96 overflow-y-auto">
      {#each filteredHotkeys as item, index (item.hotkey)}
        <div
          class="px-4 py-3 cursor-pointer border-b border-gray-100 last:border-b-0 transition-colors"
          class:bg-indigo-100={selectedIndex === index}
          class:hover:bg-gray-50={selectedIndex !== index}
          on:click={() => {
            selectedIndex = index
            submitSelection()
          }}
          on:keydown={() => {}}
          role="option"
          aria-selected={selectedIndex === index}
        >
          <div class="font-semibold text-gray-900">{item.hotkey}</div>
          <div class="text-sm text-gray-600">{item.description}</div>
        </div>
      {/each}
    </div>
  {:else if query.trim()}
    <div class="text-center py-8 text-gray-500">
      No hotkeys found matching "{query}"
    </div>
  {:else}
    <div class="text-center py-8 text-gray-500">
      Start typing to search hotkeys
    </div>
  {/if}
</div>

<style>
  :global(.max-h-96) {
    max-height: 384px;
  }
</style>
