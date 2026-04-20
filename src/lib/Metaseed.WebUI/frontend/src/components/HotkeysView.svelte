<script>
  import { afterUpdate } from "svelte";

  export let filteredHotkeys = [];
  export let query = "";
  export let selectedIndex = 0;
  export let onItemActivate = (index) => {};

  let listEl;

  export function getPageSize() {
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
</script>

<div class="bg-white rounded-lg shadow-lg p-4 w-full h-full min-h-0 flex-1 flex flex-col overflow-hidden box-border">
  {#if filteredHotkeys.length > 0}
    <div bind:this={listEl} class="flex-1 min-h-0 border border-gray-200 rounded-md overflow-y-auto">
      {#each filteredHotkeys as item, index}
        <div
          class="px-4 py-3 cursor-pointer border-b border-gray-100 last:border-b-0 transition-colors bg-gray-50"
          class:bg-indigo-100={selectedIndex === index}
          class:hover:bg-indigo-50={selectedIndex !== index}
          on:click={() => onItemActivate(index)}
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
