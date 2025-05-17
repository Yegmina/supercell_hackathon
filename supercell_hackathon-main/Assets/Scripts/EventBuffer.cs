using System.Collections.Generic;
using System.Diagnostics;

public class Event
{
    public Event(string name, string subject = null)
    {
        this.name = name;
        this.subject = subject;
    }

    public string name = "amogus";
    public string subject = null;
}

public static class EventBuffer
{
    static List<Event> events = new List<Event>();
    // For deduping events
    static Dictionary<string, List<Event>> namedEvents = new Dictionary<string, List<Event>>();

    public static void PushDedupedEvents(string category, List<Event> ev)
    {
        namedEvents[category] = ev;
    }
    
    public static void PushEvent(Event ev)
    {
        events.Add(ev);
    }

    public static List<Event> PullEvents()
    {
        var evs = events;
        events = new List<Event>();
        foreach (var v in namedEvents.Values)
            foreach (var e in v)
                evs.Add(e);

        return evs;
    }
}
