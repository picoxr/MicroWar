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
### Register event listener
Any script that needs to receive these events must register the relevant events and then obtain updates through callbacks. The following code illustrates how the registration and distribution process functions.
PlatformModels.cs
   ```csharp
    private void Start()
    {
         ...
        // Subscribe the event handler to the RoomUpdateEvent
        PlatformServiceManager.Instance.RegisterNotification<RoomUpdateEvent>(HandleRoomUpdateEvent);
        // Subscribe the event handler to the RoomListEvent
        PlatformServiceManager.Instance.RegisterNotification<RoomListEvent>(HandleRoomListEvent);
         ...
    }

    private void HandleRoomUpdateEvent(EventWrapper<RoomUpdateEvent> msg)
    {
        if (msg.Data.RoomServiceStatus != RoomServiceStatus.Idle)
        {
            ...
            if (msg.Data.RoomServiceStatus == RoomServiceStatus.InRoom) // If in a room, update the room info UI.
            {
                UpdateRoomInfo(msg.Data.CurrentRoom); // Refresh UI
            }
            return;
        }
        CreateRoomBtn.interactable = true;
        UpdateRoomInfo(msg.Data.CurrentRoom);
        //RetrieveRoomList(); 
    }

    private void OnDestroy()
    {
        if (PlatformServiceManager.Instance == null) return;
        PlatformServiceManager.Instance.UnregisterNotification<RoomUpdateEvent>(HandleRoomUpdateEvent);
        PlatformServiceManager.Instance.UnregisterNotification<RoomListEvent>(HandleRoomListEvent);
    }
   ```

## Get Service Controller Instance
By encapsulation, we can access instances that control various platform services from the ***PlatformServiceManager.cs***. Here is a code example illustrating how to retrieve these instances and how to use it.
   ```csharp
   Start()
   {
      ...
     Platformcontroller_Rooms roomController = PlatformServiceManager.Instance.GetController<PlatformController_Rooms>();
      ...
   }

   public void RetrieveRoomList()
    {
        roomController.RetrieveRoomList();
    }

    public void LeaveRoom()
    {
        roomController.LeaveRoom();
    }
   ```
The following list comprises the Event types present within the project. For more details, please see the ***PlatformModels.cs*** script.
  ```csharp
   ...
   public class ErrorEvent : EventBase{...}
   public class RoomUpdateEvent : EventBase{...}
   public class RoomListEvent : EventBase{...}
   public class RoomPlayerEvent : EventBase{...}
   ...
   ```

## More to Explore

For detailed implementation specifics and usage guidelines, please refer to the following documentation:
- [Initialization And Login](/Documentation/InitializationAndLogin.md)
- [Rooms](/Documentation/Rooms.md)
- [RTC](/Documentation/RTC.md)
- [Multiplay](/Documentation/Multiplays.md)
