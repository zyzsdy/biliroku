using System;


// Github: copyliu/bilibili_dm
namespace BiliRoku.Commentlib
{
    public delegate void DisconnectEvt(object sender, DisconnectEvtArgs e);
    public delegate void ReceivedCommentEvt(object sender, ReceivedCommentArgs e);
    public delegate void ReceivedRoomCountEvt(object sender, ReceivedRoomCountArgs e);

    public class ReceivedRoomCountArgs
    {
        public uint UserCount;
    }
    public class DisconnectEvtArgs
    {
        public Exception Error;
    }
    public class ReceivedCommentArgs
    {
        public CommentModel Comment;
    }
}
