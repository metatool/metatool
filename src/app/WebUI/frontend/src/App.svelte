<script>
  import SearchBar from './components/SearchBar.svelte'
  import { onMount } from 'svelte'
  import { initMessageListeners, sendClose, sendHotkeySelected } from './lib/messaging.js'

  let show = false
  let hotkeys = []
  let filteredHotkeys = []

  function handleClose() {
    sendClose()
  }

  function handleSearch(query) {
    console.log('Search query:', query)

    // Filter hotkeys based on query
    if (query.trim() === '') {
      filteredHotkeys = hotkeys
    } else {
      filteredHotkeys = hotkeys.filter(item =>
        item.hotkey.toLowerCase().includes(query.toLowerCase()) ||
        item.description.toLowerCase().includes(query.toLowerCase())
      )
    }
  }

  function handleSelection(selectedItem, index) {
    console.log('Selected item:', selectedItem)
    sendHotkeySelected(selectedItem, index)
  }

  onMount(() => {
    const cleanup = initMessageListeners((data) => {
      show = true
      if (Array.isArray(data.hotkeys)) {
        hotkeys = data.hotkeys
        filteredHotkeys = hotkeys
      }
    })

    return cleanup
  })
</script>

<div class="min-h-screen bg-gray-100 p-2 relative group">
    <button
      class="absolute top-1 right-1 w-6 h-6 flex items-center justify-center rounded hover:bg-gray-300 text-gray-500 hover:text-gray-700 transition-all opacity-0 group-hover:opacity-100"
      on:click={handleClose}
      title="Close"
    >
      <svg xmlns="http://www.w3.org/2000/svg" class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
      </svg>
    </button>
    <div class="mt-4 p-0 bg-white rounded">
      <SearchBar
        {hotkeys}
        {filteredHotkeys}
        onSearch={handleSearch}
        onSelection={handleSelection}
        onClose={() => show = false}
      />
    </div>
</div>
