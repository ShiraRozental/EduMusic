import logging
from flask import Blueprint, request, jsonify
from app.services.vocal_separator_service import vocal_separator_service

logger = logging.getLogger(__name__)

vocal_bp = Blueprint('vocal', __name__)


@vocal_bp.route('/separate-vocals', methods=['POST'])
def separate_vocals():
    """
    Expects JSON: { "audio_path": "/absolute/path/to/song.mp3" }
    Returns JSON: { "vocals_path": "/absolute/path/to/song_(Vocals).wav" }
    """
    data = request.get_json()

    if not data or 'audio_path' not in data:
        return jsonify({'error': 'Missing audio_path field'}), 400

    audio_path: str = data['audio_path']
    logger.info("Vocal separation requested for: %s", audio_path)

    try:
        vocals_path = vocal_separator_service.separate_vocals(audio_path)
        return jsonify({'vocals_path': vocals_path})

    except FileNotFoundError as e:
        logger.error("File not found: %s", e)
        return jsonify({'error': str(e)}), 404

    except RuntimeError as e:
        logger.error("Separation failed: %s", e)
        return jsonify({'error': str(e)}), 500
