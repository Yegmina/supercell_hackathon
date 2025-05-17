# events.py

from flask import Blueprint, request, jsonify
import threading
import random

# how many events to keep
EVENT_LIMIT = 20

# Centralized events list and API
EVENTS = [
    "cat stands and moves near X",
    "cat looks at X",
    "cat passed X",
    "cat made a sound X",
    "cat sat down",
    "IMPORTANT: cat say MEEEOOW"
]

events_bp = Blueprint("events_service", __name__)

def enforce_event_limit():
    """Trim EVENTS to the most recent EVENT_LIMIT entries."""
    global EVENTS
    if len(EVENTS) > EVENT_LIMIT:
        # drop oldest
        del EVENTS[0:len(EVENTS) - EVENT_LIMIT]

def rotate_important_flag():
    """Every 4s, remove existing IMPORTANT: flags and
    reassign it to one random event from the last five."""
    global EVENTS

    # 1) strip any existing IMPORTANT:
    EVENTS = [e.replace("IMPORTANT: ", "") for e in EVENTS]

    # 2) pick from the last five (or fewer if not yet 5)
    slice_start = max(len(EVENTS) - 5, 0)
    candidates = EVENTS[slice_start:]

    if candidates:
        choice = random.choice(candidates)
        idx = EVENTS.index(choice)
        EVENTS[idx] = f"IMPORTANT: {choice}"

    # 3) enforce history length
    enforce_event_limit()

    # 4) schedule next run
    threading.Timer(4.0, rotate_important_flag).start()

# kick off the first rotation as soon as this module is imported
rotate_important_flag()

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

    EVENTS.append(event)
    enforce_event_limit()
    return jsonify({"status": "ok", "events": EVENTS}), 200

@events_bp.route("/api/events", methods=["PUT"])
def replace_events():
    """Replace the entire global EVENTS list with a new list."""
    data = request.get_json(force=True)
    new_events = data.get("events")
    if not isinstance(new_events, list) or not all(isinstance(e, str) for e in new_events):
        return jsonify({"error": "Provide 'events' as list of strings."}), 400

    # keep only the last EVENT_LIMIT entries
    global EVENTS
    EVENTS = new_events[-EVENT_LIMIT:]
    return jsonify({"status": "replaced", "events": EVENTS}), 200
