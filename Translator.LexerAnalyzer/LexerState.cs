﻿namespace Translator.LexerAnalyzer
{
    public enum LexerState
    {
        Initial,
        Point,
        Operator,
        LessOperator,
        GreaterOperator,
        AssignmentOperator,
        Not,
        String,
        Number,
        NumberWithPoint,
        LessEqual,
        GreaterEqual,
        EqualOperator,
        NotEqual,
        Splitter,
        Comma,
        LabelDefinition,
        LabelWithSplitter,
        Hypen,
        Colon
    }
}