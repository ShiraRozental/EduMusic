from flask import Blueprint, request, jsonify
from collections import Counter
from app.services.stanza_service import stanza_service

extract_bp = Blueprint('extract', __name__)


@extract_bp.route('/extract', methods=['POST'])
def extract():
    data = request.get_json()

    if not data or 'text' not in data:
        return jsonify({'error': 'Missing text field'}), 400

    #the normalized words
    raw_words = stanza_service.extract_words(data['text'])

    #count who many time each word appears
    word_counts = Counter(raw_words)
    return jsonify({'wordCounts': dict(word_counts)})