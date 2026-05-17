import stanza
from config import Config
from app.services.stopwords_service import StopWordsService


class StanzaService:
    def __init__(self):
        # מוריד את המודל בעברית בפעם הראשונה בלבד
        stanza.download(Config.STANZA_LANG)
        self._nlp = stanza.Pipeline(
            Config.STANZA_LANG,
            processors=Config.STANZA_PROCESSORS
        )
        self._stopwords = StopWordsService()

    def extract_words(self, text: str) -> list[str]:
        if not text or not text.strip():
            return []

        doc = self._nlp(text)
        words = []

        for sentence in doc.sentences:
            for word in sentence.words:
                # לוקח את צורת הבסיס (למה), ואם אין - את המילה המקורית
                lemma = word.lemma if word.lemma else word.text
                if self._stopwords.is_valid(lemma, word.upos):
                    words.append(lemma)
        return words


# Singleton - נטען פעם אחת בלבד בהפעלת השרת
stanza_service = StanzaService()