namespace Client.Interfaces
{
    public interface IChatItem
    {
        public string ChatItemName { get; set; }
        public bool IsMe { get; set; }
    }
}
