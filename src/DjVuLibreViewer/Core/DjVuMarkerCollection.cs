using System;
using System.Collections.ObjectModel;

namespace DjVuLibreViewer.Core
{
    public class DjVuMarkerCollection : Collection<IDjVuMarker>
    {
        public event EventHandler CollectionChanged;

        protected override void ClearItems()
        {
            base.ClearItems();

            OnCollectionChanged(EventArgs.Empty);
        }

        protected override void InsertItem(int index, IDjVuMarker item)
        {
            base.InsertItem(index, item);

            OnCollectionChanged(EventArgs.Empty);
        }

        protected override void RemoveItem(int index)
        {
            base.RemoveItem(index);

            OnCollectionChanged(EventArgs.Empty);
        }

        protected override void SetItem(int index, IDjVuMarker item)
        {
            base.SetItem(index, item);

            OnCollectionChanged(EventArgs.Empty);
        }

        protected virtual void OnCollectionChanged(EventArgs e)
        {
            CollectionChanged?.Invoke(this, e);
        }
    }
}
