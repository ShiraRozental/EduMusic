class StopWordsService:
    STOP_WORDS = {
        'ו', 'או', 'אבל', 'כי', 'אם', 'אז', 'גם', 'רק',
        'של', 'על', 'אל', 'מן', 'עם', 'בין', 'לפני', 'אחרי',
        'ב', 'ל', 'מ', 'כ', 'ש',
        'אני', 'אתה', 'את', 'הוא', 'היא', 'אנחנו', 'הם', 'הן',
        'כל', 'כבר', 'עוד', 'פה', 'שם', 'כן', 'לא', 'יש', 'אין',
        'מה', 'מי', 'איך', 'למה', 'זה', 'זו', 'אלה', 'הנה',
    }

    UNWANTED_POS = {
        'PUNCT',  # פיסוק
        'NUM',  # מספרים
        'ADP',  # מילות יחס
        'CCONJ',  # מילות חיבור
        'SCONJ',  # מילות חיבור משועבדות
        'DET',  # ה' הידיעה
        'PRON',  # כינויי גוף
    }

    def is_valid(self, lemma: str, upos: str) -> bool:
        if upos in self.UNWANTED_POS:
            return False
        if lemma in self.STOP_WORDS:
            return False
        if len(lemma) < 2:
            return False
        return True