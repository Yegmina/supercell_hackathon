# events.py

from flask import Blueprint, request, jsonify

# how many events to keep
EVENT_LIMIT = 20

# Centralized events list and API

# Global EVENTS list
EVENTS = [
    "cat stands and moves near X",
    "cat looks at X",
    "cat passed X",
    "cat made a sound X",
    "cat sat down",
    "IMPORTANT: cat say MIAAAY"
]

# Blueprint for events endpoints
events_bp = Blueprint("events_service", __name__)

@events_bp.route("/api/events", methods=["GET"])
def get_events():
    """Return the full list of recent events."""
    return jsonify({"events": EVENTS}), 200

@events_bp.route("/api/events", methods=["POST"])
def add_event():
    """Append a single event and enforce max history length."""
    data = request.get_json(force=True)
    event = data.get("event", "").strip()
    if not event:
        return jsonify({"error": "No 'event' provided."}), 400

    # append new event
    EVENTS.append(event)
    # drop oldest if we exceed the limit
    if len(EVENTS) > EVENT_LIMIT:
        del EVENTS[0]

    return jsonify({"status": "ok", "events": EVENTS}), 200

@events_bp.route("/api/events", methods=["PUT"])
def replace_events():
    """Replace the entire global EVENTS list with a new list."""
    data = request.get_json(force=True)
    new_events = data.get("events")
    if not isinstance(new_events, list) or not all(isinstance(e, str) for e in new_events):
        return jsonify({"error": "Provide 'events' as list of strings."}), 400

    # enforce limit on replacement too
    global EVENTS
    EVENTS = new_events[-EVENT_LIMIT:]
    return jsonify({"status": "replaced", "events": EVENTS}), 200
