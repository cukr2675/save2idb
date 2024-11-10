using UnityEngine;

namespace SimpleFileBrowser
{
	[RequireComponent( typeof( RectTransform ) )]
	public class IDBA_ListItem : MonoBehaviour
	{
		public object Tag { get; set; }
		public int Position { get; set; }

		private IListViewAdapter adapter;

		internal void SetAdapter( IListViewAdapter listView )
		{
			this.adapter = listView;
		}

		public void OnClick()
		{
			if( adapter.OnItemClicked != null )
				adapter.OnItemClicked( this );
		}
	}
}