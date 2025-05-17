
from flask import Blueprint, request, jsonify
import logging

# Configure module-level logger for states
state_logger = logging.getLogger("state_service")
state_logger.setLevel(logging.DEBUG)

# Global STATES list (current snapshot)
STATES = [
    "cat looking at the door",
    "cat near table",
    "cat sniffing floor"
]

# Blueprint for states endpoints
states_bp = Blueprint("states_service", __name__)

@states_bp.route("/api/states", methods=["GET"])
def get_states():
    """Return the full list of current states."""
    state_logger.debug(f"Returning STATES: {STATES}")
    return jsonify({"states": STATES}), 200

@states_bp.route("/api/states", methods=["POST"])
def add_state():
    """Append a single state to the global STATES list."""
    data = request.get_json(force=True)
    state = data.get("state", "").strip()
    if not state:
        return jsonify({"error": "No 'state' provided."}), 400
    STATES.append(state)
    state_logger.debug(f"State added: {state}. Current STATES: {STATES}")
    return jsonify({"status": "ok", "states": STATES}), 200

@states_bp.route("/api/states", methods=["PUT"])
def replace_states():
    """Replace the entire global STATES list with a new list."""
    data = request.get_json(force=True)
    new_states = data.get("states")
    if not isinstance(new_states, list) or not all(isinstance(s, str) for s in new_states):
        return jsonify({"error": "Provide 'states' as list of strings."}), 400
    global STATES
    STATES = new_states
    state_logger.debug(f"States replaced. New STATES: {STATES}")
    return jsonify({"status": "replaced", "states": STATES}), 200