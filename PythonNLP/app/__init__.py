from flask import Flask
from app.routes.extract_routes import extract_bp

def create_app():
    app = Flask(__name__)
    app.register_blueprint(extract_bp)
    return app