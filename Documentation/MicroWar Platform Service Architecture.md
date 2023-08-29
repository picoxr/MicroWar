# MicroWar Platform Service Architecture

Considering the maintainability and scalability of the program, we have encapsulated the PICO Platform APIs. This encapsulation ensures relatively independent logic for each component while providing a unified access approach for all feature's instance. By utilizing an ***event dispatc system***, we have interconnected these components to reduce coupling between modules.

In this architecture, specific scripts implement the logic for different services. The PlatformServiceManager serves as an interface for accessing various instances. Here is the platform service structure diagram used in the MicroWar project:

![PICO Platform Service Structure](/Documentation/Files/PlatformServiceStructure.jpg)
## Event Dispatch
This chapter will delve into the operational principles of the event dispatch system employed in MicroWar, and how it achieves the distribution of various data types through a unified interface.
### EventWrapper 
We have introduced the ***EventWrapper*** to encapsulate various types of data and facilitate distribution through a generic interface. Here is the definition of EventWrapper:

PlatformModels.cs
   
   ```csharp
    public class EventWrapper<T>: EventWrapper
    {
        public delegate void Handler(EventWrapper<T> EventData);
        public readonly T Data;
        public EventWrapper(NotificationType notificationType, T data):base( notificationType)
        {
            Data = data;
        }
    }
   ```
The example below explains how to define a foundational event type. We generate these data instances when receiving callbacks from the PICO platform and subsequently distribute them to listeners.

PlatformModels.cs
   ```csharp
    public class RoomInfo
       {
           public readonly NotificationType NotificationType;
           public readonly RoomServiceStatus RoomStatus;
           public readonly RoomListRetrieveStatus RoomListRetrieveStatus;
           public readonly Room CurrentRoom;
           public readonly RoomList RoomList;
           public readonly User UpdatePlayer;
           //Get Room Data method
           public RoomInfo(RoomServiceStatus status, Room currentRoom, RoomListRetrieveStatus roomListRetrieveStatus, RoomList roomList) 
           {
               CurrentRoom = currentRoom;
               RoomStatus = status;
               RoomListRetrieveStatus = roomListRetrieveStatus;
               RoomList = roomList;
           }
       }
   ```
Any script that wishes to receive these events must register the relevant events and then obtain updates through callbacks. The following code illustrates how the registration and distribution process functions.
PlatformModels.cs
   ```csharp
        /// <summary>
        /// Register a callback to receive platform event
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="handler"></param>
        public void RegisterNotification<T>(EventWrapper<T>.Handler handler)
        {
            if(handler == null)
            {
                DebugUtils.LogError(nameof(PlatformServiceManager),"Can't Register A Handler That Is Null !");
                return;
            }

            if ( ! eventMap.ContainsKey( typeof(T) ))
            {
                eventMap.Add(typeof(T), new List<Delegate>());
            }

            if ( ! eventMap[typeof(T)].Contains(handler))
            {
                eventMap[typeof(T)].Add(handler);
            }
        }

        public void NotifyEventHandler<T>(EventWrapper<T> data)
        {
            List<Delegate> handlerList = null;
            if( eventMap.ContainsKey(typeof(T)))
            {
                handlerList = eventMap[typeof(T)];
            }
        
            if(handlerList!=null)
            {
                for (int i = 0; i < handlerList.Count; i++)
                {
                    if(handlerList[i]!= null && handlerList[i].Target != null)
                    {
                        handlerList[i].DynamicInvoke(data);
                    }
                }
            }
        }
   ```

## Get Service Instance

## More to Explore

For detailed implementation specifics and usage guidelines, please refer to the following documentation:
- [Rooms](/Documentation/Rooms.md)
- [RTC](/Documentation/RTC.md)
- [Multiplay](/Documentation/Multiplays.md)
