from flask import Flask
from app.routes.extract_routes import extract_bp
from app.routes.vocal_routes import vocal_bp
def create_app():
    app = Flask(__name__)
    app.register_blueprint(extract_bp)
    app.register_blueprint(vocal_bp)
    return app