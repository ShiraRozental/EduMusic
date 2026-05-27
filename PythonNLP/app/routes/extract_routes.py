import logging
from flask import Blueprint, request, jsonify
from collections import Counter
from app.services.stanza_service import stanza_service

logger = logging.getLogger(__name__)

extract_bp = Blueprint('extract', __name__)


@extract_bp.route('/extract', methods=['POST'])
def extract():
    data = request.get_json()

    if not data or 'text' not in data:
        return jsonify({'error': 'Missing text field'}), 400

    text: str = data['text']

    logger.info("Received text for extraction: %d chars", len(text))

    #the normalized words
    raw_words = stanza_service.extract_words(text)

    #count who many time each word appears
    word_counts = Counter(raw_words)
    logger.info("Extracted %d unique words from %d total tokens", len(word_counts), len(raw_words))

    return jsonify({'wordCounts': dict(word_counts)})