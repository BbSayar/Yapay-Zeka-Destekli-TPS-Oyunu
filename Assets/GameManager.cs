using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement; // Sahne yonetimi
using TMPro;                     // TextMeshPro
using static System.Net.Mime.MediaTypeNames;
using System.Diagnostics;

public class GameManager : MonoBehaviour
{
    // Oyunun o anki durumunu takip et
    public enum OyunDurumu { Giris, Oynaniyor, Duraklatildi, Bitti }
    public static OyunDurumu mevcutDurum;

    [Header("Durum Panelleri")]
    // Hiyerarsiden surukleyip birakilacak ana paneller
    public GameObject girisMenuPanel;
    public GameObject duraklatmaMenuPanel;
    public GameObject oyunBittiPanel;
    public GameObject ayarlarPanel; // Ayarlar üst panel

    [Header("Paylasilan UI Elemanlar")]
    // Shared_UI_Group i�indeki butonlar
    public GameObject btnSesAc;  
    public GameObject btnSesKapa;

    [Header("Oyun Bitti Ekran")]
    // oyunBittiPanel icindeki text
    public TextMeshProUGUI sonucText; 

    void Start()
    {
        // Oyuna giris menusuyle basla
        GirisDurumuAyarlari();
        // Sesi varsayilan olarak ac
        SesiAc();
    }

    void Update()
    {
        // ESC tusu yonetimi
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            // ayarlar paneli aciksa ESC tusu once ayarlari kapat
            if (ayarlarPanel.activeInHierarchy)
            {
                AyarlariKapat();
            }
            // oyun oynaniyorsa oyunu duraklat
            else if (mevcutDurum == OyunDurumu.Oynaniyor)
            {
                OyunuDuraklat();
            }
            //  oyun duraklatildiysa oyuna devam et
            else if (mevcutDurum == OyunDurumu.Duraklatildi)
            {
                OyunaDevamEt();
            }
        }
    }

    void TumDurumPanelleriniKapat()
    {
        girisMenuPanel.SetActive(false);
        duraklatmaMenuPanel.SetActive(false);
        oyunBittiPanel.SetActive(false);
    }

    // --- Durum Fonksiyonlari ---

    void GirisDurumuAyarlari()
    {
        mevcutDurum = OyunDurumu.Giris;
        Time.timeScale = 0f; // Menudeyken zamani durdur
        TumDurumPanelleriniKapat();
        girisMenuPanel.SetActive(true);
    }

    public void OyunuBaslat()
    {
        mevcutDurum = OyunDurumu.Oynaniyor;
        Time.timeScale = 1f; // Zamani baslat
        TumDurumPanelleriniKapat();
    }

    void OyunuDuraklat()
    {
        mevcutDurum = OyunDurumu.Duraklatildi;
        Time.timeScale = 0f; // Zamani durdur
        TumDurumPanelleriniKapat();
        duraklatmaMenuPanel.SetActive(true);
    }

    public void OyunaDevamEt()
    {
        mevcutDurum = OyunDurumu.Oynaniyor;
        Time.timeScale = 1f; // Zamani devam ettir
        TumDurumPanelleriniKapat();
    }

    public void OyunuBitir(bool kazandiMi)
    {
        mevcutDurum = OyunDurumu.Bitti;
        Time.timeScale = 0f;
        TumDurumPanelleriniKapat();
        oyunBittiPanel.SetActive(true);

        if (sonucText != null)
        {
            if (kazandiMi)
            {
                sonucText.text = "GOREV BASARILI!";
            }
            else
            {
                sonucText.text = "GÖREV BASARISIZ!";   
            }
        }
    }

    // --- Buton Fonksiyonlarİ ---

    public void SahneyiYenidenYukle()
    {
        Time.timeScale = 1f; // Sahne yUklenmeden zamani duzelt
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void OyundanCik()
    {

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif

    }
    //Overlay Panel Fonksiyonlari

    public void AyarlariAc()
    {
        ayarlarPanel.SetActive(true);
        Time.timeScale = 0f; // Ayarlar acikken oyun her zaman durur
    }

    public void AyarlariKapat()
    {
        ayarlarPanel.SetActive(false);

        if (mevcutDurum == OyunDurumu.Oynaniyor)
        {
            Time.timeScale = 1f;
        }
    }

    public void SesiAc()
    {
        AudioListener.volume = 1f; 
        btnSesAc.SetActive(true);  
        btnSesKapa.SetActive(false); 
    }

    public void SesiKapat()
    {
        AudioListener.volume = 0f;
        btnSesAc.SetActive(false);
        btnSesKapa.SetActive(true);
    }
}