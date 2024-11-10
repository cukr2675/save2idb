namespace SimpleFileBrowser
{
	public delegate void OnItemClickedHandler( IDBA_ListItem item );

	public interface IListViewAdapter
	{
		OnItemClickedHandler OnItemClicked { get; set; }

		int Count { get; }
		float ItemHeight { get; }

		IDBA_ListItem CreateItem();

		void SetItemContent( IDBA_ListItem item );
	}
}