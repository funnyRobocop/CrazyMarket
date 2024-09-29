using UnityEngine;
using System;

public class SimpleScreenshotCapture : MonoBehaviour
{
    private void Update()
    {
        // Проверяем, была ли нажата клавиша P
        if (Input.GetKeyDown(KeyCode.P))
        {
            CaptureScreenshot();
        }
    }

    private void CaptureScreenshot()
    {
        // Создаем имя файла с текущей датой и временем
        string fileName = $"Screenshot_{DateTime.Now:yyyyMMdd_HHmmss}.png";

        // Захватываем скриншот
        ScreenCapture.CaptureScreenshot(fileName);

        Debug.Log($"Скриншот сохранен: {fileName}");
    }
}