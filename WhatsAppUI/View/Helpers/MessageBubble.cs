namespace WhatsAppUI.View.Helpers
{
    public class MessageBubble
    {
        public string Text { get; set; }
        public bool IsMe { get; set; }

        public MessageBubble(string txt, bool isMe)
        {
            Text = txt;
            IsMe = isMe;
        }
    }
}
