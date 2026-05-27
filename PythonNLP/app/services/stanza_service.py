import stanza
from config import Config
from app.services.stopwords_service import StopWordsService


class StanzaService:
    def __init__(self):
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

        unknown_words: list[str] = []

        for sentence in doc.sentences:
            for word in sentence.words:
                if word.lemma:
                    lemma = word.lemma
                else:
                    # מתעדים שהמילה לא הוכרה
                    unknown_words.append(word.text)
                    lemma = word.text  # fallback to original, but we log it

                if self._stopwords.is_valid(lemma, word.upos):
                    words.append(lemma)

                    # log unknown words once per call to avoid flooding
                if unknown_words:
                    logger.warning(
                        "Stanza could not lemmatize %d word(s): %s",
                        len(unknown_words),
                        ", ".join(unknown_words[:20])  # cap at 20 to avoid huge log lines
                    )

                return words


# Singleton - נטען פעם אחת בלבד בהפעלת השרת
stanza_service = StanzaService()