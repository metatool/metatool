<script>
  import { onMount } from "svelte";
  import { afterUpdate } from "svelte";
  let query = "";
  let inputEl;
  let listEl;
  let selectedIndex = 0;

  export let hotkeys = [];
  export let filteredHotkeys = [];

  // $: console.log('SearchBar received filteredHotkeys:', filteredHotkeys, 'length:', filteredHotkeys?.length);
  export let onSearch = (query) => {};
  export let onSelection = (item, index) => {};
  export let onClose = () => {};

  function handleInput(e) {
    query = e.target.value;
    onSearch(query);

    selectedIndex = 0; // Reset selection when typing
  }

  function selectItem(index) {
    selectedIndex = index;
  }

  function submitSelection() {
    if (selectedIndex >= 0 && filteredHotkeys[selectedIndex]) {
      const item = filteredHotkeys[selectedIndex];
      onSelection(item, selectedIndex);
      query = "";
      //selectedIndex = 0;
    }
  }

  function onKey(e) {
    if (e.key === "Escape") {
      onClose();
    } else if (e.key === "Enter") {
      submitSelection();
    } else if (e.key === "ArrowDown") {
      e.preventDefault();
      selectedIndex = Math.min(selectedIndex + 1, filteredHotkeys.length - 1);
    } else if (e.key === "ArrowUp") {
      e.preventDefault();
      selectedIndex = Math.max(selectedIndex - 1, 0);
    } else if (e.key === "Home") {
      e.preventDefault();
      selectedIndex = 0;
    } else if (e.key === "End") {
      e.preventDefault();
      selectedIndex = filteredHotkeys.length - 1;
    } else if (e.key === "PageDown") {
      e.preventDefault();
      const pageSize = getPageSize();
      selectedIndex = Math.min(selectedIndex + pageSize, filteredHotkeys.length - 1);
    } else if (e.key === "PageUp") {
      e.preventDefault();
      const pageSize = getPageSize();
      selectedIndex = Math.max(selectedIndex - pageSize, 0);
    }
  }

  function getPageSize() {
    if (!listEl || !listEl.children.length) return 5;
    const itemHeight = listEl.children[0].offsetHeight;
    return Math.max(1, Math.floor(listEl.clientHeight / itemHeight));
  }

  afterUpdate(() => {
    if (listEl && selectedIndex >= 0) {
      const item = listEl.children[selectedIndex];
      if (item) item.scrollIntoView({ block: "nearest" });
    }
  });

  function focusHandler() {
    if (inputEl) inputEl.focus();
  }

  onMount(() => {
    const listener = () => focusHandler();
    document.addEventListener("focus-search", listener);
    setTimeout(focusHandler, 30);
    return () => document.removeEventListener("focus-search", listener);
  });
</script>

<div class="bg-white rounded-lg shadow-lg p-4 max-w-2xl mx-auto">
  <div class="flex items-center gap-3 mb-4">
    <input
      bind:this={inputEl}
      bind:value={query}
      on:input={handleInput}
      on:keydown={onKey}
      on:blur={() => inputEl?.focus()}
      class="flex-1 px-4 py-3 rounded-md border border-gray-200 focus:outline-none focus:ring-2 focus:ring-indigo-500"
      placeholder="Search hotkeys... (use ↑↓ to navigate, Enter to execute)"
      autocomplete="off"
    />
  </div>

  {#if filteredHotkeys.length > 0}
    <div bind:this={listEl} class="border border-gray-200 rounded-md max-h-96 overflow-y-auto">
      {#each filteredHotkeys as item, index}
        <div
          class="px-4 py-3 cursor-pointer border-b border-gray-100 last:border-b-0 transition-colors bg-gray-50"
          class:bg-indigo-100={selectedIndex === index}
          class:hover:bg-indigo-50={selectedIndex !== index}
          on:click={() => {
            selectedIndex = index;
            submitSelection();
          }}
          on:keydown={() => {}}
          role="option"
          aria-selected={selectedIndex === index}
          tabindex="0"
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
