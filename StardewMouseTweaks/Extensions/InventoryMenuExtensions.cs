using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace StardewMouseTweaks.Extensions;

public static class InventoryMenuExtensions
{
    public static bool TryGetHoveredItemSlot(this InventoryMenu menu, ICursorPosition position, [NotNullWhen(true)] out InventorySlot? slot)
    {
        foreach (var candidateSlot in menu) {
            if (!candidateSlot.ClickableComponent.ContainsPoint(position)) continue;

            slot = candidateSlot;
            return true;
        }

        slot = null;
        return false;
    }

    public static InventorySlot GetItemSlot(this InventoryMenu menu, int slotIndex)
    {
        return new InventorySlot {
            InventoryMenu = menu,
            Index = slotIndex,
        };
    }

    public static InventoryMenuEnumerator GetEnumerator(this InventoryMenu menu) => new InventoryMenuEnumerator(menu);

    public record InventorySlot
    {
        public required InventoryMenu InventoryMenu { get; init; }
        public required int Index { get; init; }

        public ClickableComponent ClickableComponent => InventoryMenu.inventory[Index];

        public Item? Item {
            get => InventoryMenu.actualInventory[Index];
            set => InventoryMenu.actualInventory[Index] = value;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(InventoryMenu, Index);
        }

        public virtual bool Equals(InventorySlot? other)
        {
            if (other is null) return false;
            if (!ReferenceEquals(InventoryMenu, other.InventoryMenu)) return false;
            if (Index != other.Index) return false;
            return true;
        }
    }

    public struct InventoryMenuEnumerator : IEnumerator<InventorySlot>
    {
        private InventoryMenu _inventoryMenu;
        private int _index;

        internal InventoryMenuEnumerator(InventoryMenu inventoryMenu)
        {
            _inventoryMenu = inventoryMenu;
            _index = -1;
        }

        public bool MoveNext()
        {
            _index += 1;
            return _index < _inventoryMenu.capacity;
        }

        public void Reset() => _index = -1;

        public InventorySlot Current {
            get {
                try {
                    return _inventoryMenu.GetItemSlot(_index);
                }
                catch (IndexOutOfRangeException) {
                    if (_index < 0)
                        throw new InvalidOperationException("Cannot retrieve element before advancing position from initial state.");
                    throw;
                }
            }
        }

        object IEnumerator.Current => Current;

        public void Dispose() { }
    }
}
