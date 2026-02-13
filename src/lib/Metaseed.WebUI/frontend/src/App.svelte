<script>
  import SearchBar from './components/SearchBar.svelte'
  import CloseButton from './components/CloseButton.svelte'
  import { onMount } from 'svelte'
  import { initMessageListeners, sendClose, sendHotkeySelected } from './lib/messaging.js'

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
      if (Array.isArray(data.hotkeys)) {
        hotkeys = data.hotkeys
        filteredHotkeys = hotkeys
      }
      // Focus the search input when the window is shown
      setTimeout(() => document.dispatchEvent(new CustomEvent('focus-search')), 50)
    })

    return cleanup
  })
</script>

<div class="min-h-screen bg-gray-100 p-2 relative group">
    <CloseButton onClick={handleClose} />
    <div class="mt-4 p-0 bg-white rounded">
      <SearchBar
        {hotkeys}
        {filteredHotkeys}
        onSearch={handleSearch}
        onSelection={handleSelection}
        onClose={handleClose}
      />
    </div>
</div>
