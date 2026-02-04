<script>
  import { createEventDispatcher, onMount } from 'svelte'
  const dispatch = createEventDispatcher()
  let query = ''
  let inputEl

  function submit(){
    if (query.trim()) {
      dispatch('search', query.trim())
      query = ''
    }
  }
  function onKey(e){
    if (e.key === 'Escape') {
      dispatch('close')
    }
    if (e.key === 'Enter') {
      submit()
    }
  }
  function focusHandler(){
    if (inputEl) inputEl.focus()
  }

  onMount(()=>{
    const listener = () => focusHandler()
    document.addEventListener('focus-search', listener)
    setTimeout(focusHandler, 30)
    return () => document.removeEventListener('focus-search', listener)
  })
</script>

<div class="bg-white rounded-lg shadow-lg p-4">
  <div class="flex items-center gap-3">
    <input
      bind:this={inputEl}
      bind:value={query}
      on:keydown={onKey}
      class="flex-1 px-4 py-3 rounded-md border border-gray-200 focus:outline-none focus:ring-2 focus:ring-indigo-500"
      placeholder="Search..."
    />
    <button
      on:click={submit}
      class="bg-indigo-600 text-white px-4 py-2 rounded-md hover:bg-indigo-700"
    >
      Go
    </button>
  </div>
</div>
