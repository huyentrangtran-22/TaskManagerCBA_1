using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskManager.Notifications.Abstractions
{
    public enum NotificationEventType
    {
        UserMentioned,
        AddedAsMemberToCard,
        AddedAsMemberToBoard,
        CommentReplied,
        DueDateReminder,
        WatchedItemChanged
    }
}
