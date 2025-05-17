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

    public override string ToString() {
        return name + " " + subject;
    }
}

public static class EventBuffer
{
    static List<Event> events = new List<Event>();
    // For deduping events
    public static Dictionary<string, List<string>> state = new Dictionary<string, List<string>>();

    public static void SetState(string category, List<string> ev)
    {
        state[category] = ev;
    }
    
    public static void PushEvent(Event ev)
    {
        events.Add(ev);
    }

    public static List<Event> PullEvents()
    {
        var evs = events;
        events = new List<Event>();
        return evs;
    }
}
