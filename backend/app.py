from flask import Flask, request, render_template, send_from_directory, jsonify, session
from flask_jwt_extended import JWTManager
from events import events_bp
from anim_service import anim_bp
from states import states_bp

from config import config
import os
from datetime import timedelta
import time

from utils.logger import logger

# Create the Flask app instance
app = Flask(__name__)
app.register_blueprint(anim_bp)
app.register_blueprint(events_bp)

app.register_blueprint(states_bp)


@app.errorhandler(Exception)
def handle_unknown_error(e):
    logger.error(f"Unknown error occurred: {str(e)}")
    return jsonify({"success": False, "finished": True, "message": "Unknown error occurred"}), 500


"""basic functions"""


@app.route("/")
def index():
    """Launch  main page"""
    try:
        return "wait a minute, who are you"
    except Exception as e:
        # Log the error for debugging
        app.logger.error(f"Error rendering index.html: {e}")
        # Return a user-friendly error message
        return jsonify({"error": "Unable to load the page. Probably index.html is missing"}), 500


@app.route("/locales/<path:filename>")
def serve_locale(filename):
    """Send localization json files (en.json&fi.json) from the 'locales' directory."""
    return send_from_directory("locales", filename)


@app.route('/static/videos/<filename>')
def serve_video(filename):
    """Serve static vidoe files from the 'static' directory."""
    return send_from_directory('static/videos', filename, mimetype='video/mp4')


# Static files route
@app.route('/static/<path:filename>')
def serve_static(filename):
    """Serve static files from the 'static' directory."""
    try:
        return send_from_directory(os.path.join(app.root_path, 'static'), filename)
    except Exception as e:
        app.logger.error(f"Error serving static file {filename}: {e}")
        return jsonify({"error": f"Unable to serve {filename}"}), 500


@app.errorhandler(404)
def page_not_found(e):
    """404 error handler."""
    return jsonify({"error": "Page not found"}), 404


@app.errorhandler(500)
def internal_server_error(e):
    """500 error handler."""
    return jsonify({"error": "Internal server error. "}), 500


if __name__ == "__main__":
    # Launch flask in localhost, in debug mode.
    app.run(host="127.0.0.1", port=5000, debug=True)
