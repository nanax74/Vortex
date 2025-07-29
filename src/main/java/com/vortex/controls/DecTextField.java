package com.vortex.controls;

import javafx.scene.control.TextField;
import javafx.scene.control.TextFormatter;

import java.util.function.UnaryOperator;

public class DecTextField extends TextField
{
    public DecTextField()
    {
        UnaryOperator<TextFormatter.Change> filter = change ->
        {
            String newText = change.getControlNewText();
            if (newText.matches("\\d*"))
            {  // 数字のみ許可
                return change;
            }
            return null;
        };
        setTextFormatter(new TextFormatter<>(filter));
    }

    public int getIntValue()
    {
        return getText().isEmpty() ? 0 : Integer.parseInt(getText(), 10);
    }

    public void setIntValue(int value)
    {
        setText(String.format("%d", value));
    }
}
