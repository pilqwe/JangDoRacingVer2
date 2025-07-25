using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [Header("배경 음악")]
    public AudioSource backgroundMusicSource;
    public AudioClip backgroundMusic;
    
    [Header("게임 시작 사운드")]
    public AudioSource countdownSoundSource;
    public AudioClip countdownSound; // 3-2-1 카운트다운 사운드
    public AudioClip goSound; // Go! 사운드
    
    [Header("스쿠터 엔진 사운드")]
    public AudioSource engineSoundSource;
    public AudioClip engineIdleSound; // 기본 엔진음
    public AudioClip engineRunningSound; // 달리는 엔진음
    
    [Header("스쿠터 부스터 사운드")]
    public AudioSource boosterSoundSource;
    public AudioClip boosterStartSound; // 부스터 시작 사운드
    public AudioClip boosterLoopSound; // 부스터 지속 사운드
    public AudioClip boosterEndSound; // 부스터 종료 사운드
    
    [Header("스쿠터 드리프트 사운드")]
    public AudioSource driftSoundSource;
    public AudioClip driftStartSound; // 드리프트 시작 사운드
    public AudioClip driftLoopSound; // 드리프트 지속 사운드
    public AudioClip driftEndSound; // 드리프트 종료 사운드
    
    [Header("아이템 사운드")]
    public AudioSource itemSoundSource;
    public AudioClip itemPickupSound; // 아이템 획득 사운드
    public AudioClip boostItemSound; // 부스트 아이템 특별 사운드
    public AudioClip inkItemSound; // 먹물 아이템 특별 사운드
    
    [Header("UI 사운드")]
    public AudioSource uiSoundSource;
    public AudioClip buttonClickSound; // 버튼 클릭 사운드
    public AudioClip raceCompleteSound; // 레이스 완료 사운드
    
    [Header("사운드 설정")]
    [Range(0f, 1f)]
    public float masterVolume = 1f;
    [Range(0f, 1f)]
    public float musicVolume = 0.7f;
    [Range(0f, 1f)]
    public float sfxVolume = 1f;
    
    public static SoundManager Instance;
    
    private bool isEngineRunning = false;
    private bool isDrifting = false;
    private bool isBoosting = false;

    void Awake()
    {
        // 싱글톤 패턴
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        // 오디오 소스 초기화
        InitializeAudioSources();
        
        // 배경음악 시작
        PlayBackgroundMusic();
        
        Debug.Log("🎵 SoundManager 초기화 완료!");
    }

    void InitializeAudioSources()
    {
        // 오디오 소스가 null인 경우 자동으로 생성
        if (backgroundMusicSource == null)
            backgroundMusicSource = gameObject.AddComponent<AudioSource>();
        
        if (countdownSoundSource == null)
            countdownSoundSource = gameObject.AddComponent<AudioSource>();
        
        if (engineSoundSource == null)
            engineSoundSource = gameObject.AddComponent<AudioSource>();
        
        if (boosterSoundSource == null)
            boosterSoundSource = gameObject.AddComponent<AudioSource>();
        
        if (driftSoundSource == null)
            driftSoundSource = gameObject.AddComponent<AudioSource>();
        
        if (itemSoundSource == null)
            itemSoundSource = gameObject.AddComponent<AudioSource>();
        
        if (uiSoundSource == null)
            uiSoundSource = gameObject.AddComponent<AudioSource>();
        
        // 배경음악 설정
        backgroundMusicSource.loop = true;
        backgroundMusicSource.volume = musicVolume * masterVolume;
        
        // 엔진 사운드 설정
        engineSoundSource.loop = true;
        engineSoundSource.volume = sfxVolume * masterVolume * 0.8f;
        
        // 부스터 사운드 설정
        boosterSoundSource.volume = sfxVolume * masterVolume;
        
        // 드리프트 사운드 설정
        driftSoundSource.loop = false;
        driftSoundSource.volume = sfxVolume * masterVolume * 0.9f;
        
        // 아이템 사운드 설정
        itemSoundSource.volume = sfxVolume * masterVolume;
        
        // UI 사운드 설정
        uiSoundSource.volume = sfxVolume * masterVolume * 0.8f;
    }

    // 배경음악 재생
    public void PlayBackgroundMusic()
    {
        if (backgroundMusic != null && backgroundMusicSource != null)
        {
            backgroundMusicSource.clip = backgroundMusic;
            backgroundMusicSource.Play();
            Debug.Log("🎵 배경음악 재생 시작!");
        }
    }

    // 배경음악 정지
    public void StopBackgroundMusic()
    {
        if (backgroundMusicSource != null)
        {
            backgroundMusicSource.Stop();
            Debug.Log("🎵 배경음악 정지!");
        }
    }

    // 카운트다운 사운드 재생 (3, 2, 1)
    public void PlayCountdownSound()
    {
        if (countdownSound != null && countdownSoundSource != null)
        {
            countdownSoundSource.clip = countdownSound;
            countdownSoundSource.volume = sfxVolume * masterVolume;
            countdownSoundSource.Play();
            Debug.Log("🔢 카운트다운 사운드 재생!");
        }
    }

    // Go! 사운드 재생
    public void PlayGoSound()
    {
        if (goSound != null && countdownSoundSource != null)
        {
            countdownSoundSource.clip = goSound;
            countdownSoundSource.volume = sfxVolume * masterVolume;
            countdownSoundSource.Play();
            Debug.Log("🚀 Go! 사운드 재생!");
        }
    }

    // 엔진 사운드 시작
    public void StartEngineSound()
    {
        if (!isEngineRunning && engineRunningSound != null && engineSoundSource != null)
        {
            engineSoundSource.clip = engineRunningSound;
            engineSoundSource.Play();
            isEngineRunning = true;
            Debug.Log("🏎️ 엔진 사운드 시작!");
        }
    }

    // 엔진 사운드 정지
    public void StopEngineSound()
    {
        if (isEngineRunning && engineSoundSource != null)
        {
            engineSoundSource.Stop();
            isEngineRunning = false;
            Debug.Log("🏎️ 엔진 사운드 정지!");
        }
    }

    // 엔진 사운드 피치 조절 (속도에 따라)
    public void UpdateEngineSound(float speedRatio)
    {
        if (isEngineRunning && engineSoundSource != null)
        {
            engineSoundSource.pitch = Mathf.Lerp(0.4f, 1.6f, speedRatio);
            engineSoundSource.volume = Mathf.Lerp(0.1f, 0.5f, speedRatio) * sfxVolume * masterVolume;
        }
    }

    // 부스터 사운드 시작   
    public void StartBoosterSound()
    {
        if (!isBoosting && boosterSoundSource != null)
        {
            if (boosterStartSound != null)
            {
                boosterSoundSource.clip = boosterStartSound;
                boosterSoundSource.Play();
            }
            
            // 지속 사운드가 있으면 0.2초 후에 재생
            if (boosterLoopSound != null)
            {
                Invoke(nameof(PlayBoosterLoopSound), 0.2f);
            }
            
            isBoosting = true;
            Debug.Log("🚀 부스터 사운드 시작!");
        }
    }

    void PlayBoosterLoopSound()
    {
        if (isBoosting && boosterLoopSound != null && boosterSoundSource != null)
        {
            boosterSoundSource.clip = boosterLoopSound;
            boosterSoundSource.loop = true;
            boosterSoundSource.Play();
        }
    }

    // 부스터 사운드 종료
    public void EndBoosterSound()
    {
        if (isBoosting && boosterSoundSource != null)
        {
            boosterSoundSource.Stop();
            boosterSoundSource.loop = false;
            
            if (boosterEndSound != null)
            {
                boosterSoundSource.clip = boosterEndSound;
                boosterSoundSource.Play();
            }
            
            isBoosting = false;
            Debug.Log("🚀 부스터 사운드 종료!");
        }
    }

    // 드리프트 사운드 시작
    public void StartDriftSound()
    {
        if (!isDrifting && driftSoundSource != null)
        {
            if (driftStartSound != null)
            {
                driftSoundSource.clip = driftStartSound;
                driftSoundSource.Play();
            }
            
            // 지속 사운드가 있으면 0.3초 후에 재생
            if (driftLoopSound != null)
            {
                Invoke(nameof(PlayDriftLoopSound), 0.3f);
            }
            
            isDrifting = true;
            Debug.Log("🏎️ 드리프트 사운드 시작!");
        }
    }

    void PlayDriftLoopSound()
    {
        if (isDrifting && driftLoopSound != null && driftSoundSource != null)
        {
            driftSoundSource.clip = driftLoopSound;
            driftSoundSource.loop = true;
            driftSoundSource.Play();
        }
    }

    // 드리프트 사운드 종료
    public void EndDriftSound()
    {
        if (isDrifting && driftSoundSource != null)
        {
            driftSoundSource.Stop();
            driftSoundSource.loop = false;
            
            if (driftEndSound != null)
            {
                driftSoundSource.clip = driftEndSound;
                driftSoundSource.Play();
            }
            
            isDrifting = false;
            Debug.Log("🏎️ 드리프트 사운드 종료!");
        }
    }

    // 아이템 획득 사운드 재생
    public void PlayItemPickupSound()
    {
        if (itemPickupSound != null && itemSoundSource != null)
        {
            itemSoundSource.clip = itemPickupSound;
            itemSoundSource.Play();
            Debug.Log("💎 아이템 획득 사운드 재생!");
        }
    }

    // 부스트 아이템 특별 사운드
    public void PlayBoostItemSound()
    {
        if (boostItemSound != null && itemSoundSource != null)
        {
            itemSoundSource.clip = boostItemSound;
            itemSoundSource.Play();
            Debug.Log("⚡ 부스트 아이템 사운드 재생!");
        }
    }

    // 먹물 아이템 특별 사운드
    public void PlayInkItemSound()
    {
        if (inkItemSound != null && itemSoundSource != null)
        {
            itemSoundSource.clip = inkItemSound;
            itemSoundSource.Play();
            Debug.Log("🖤 먹물 아이템 사운드 재생!");
        }
    }

    // 버튼 클릭 사운드
    public void PlayButtonClickSound()
    {
        if (buttonClickSound != null && uiSoundSource != null)
        {
            uiSoundSource.clip = buttonClickSound;
            uiSoundSource.Play();
            Debug.Log("🔘 버튼 클릭 사운드 재생!");
        }
    }

    // 레이스 완료 사운드
    public void PlayRaceCompleteSound()
    {
        if (raceCompleteSound != null && uiSoundSource != null)
        {
            uiSoundSource.clip = raceCompleteSound;
            uiSoundSource.Play();
            Debug.Log("🏆 레이스 완료 사운드 재생!");
        }
    }

    // 마스터 볼륨 설정
    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        UpdateAllVolumes();
    }

    // 음악 볼륨 설정
    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        if (backgroundMusicSource != null)
            backgroundMusicSource.volume = musicVolume * masterVolume;
    }

    // 효과음 볼륨 설정
    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        UpdateAllVolumes();
    }

    void UpdateAllVolumes()
    {
        if (backgroundMusicSource != null)
            backgroundMusicSource.volume = musicVolume * masterVolume;
        
        if (engineSoundSource != null)
            engineSoundSource.volume = sfxVolume * masterVolume * 0.8f;
        
        if (boosterSoundSource != null)
            boosterSoundSource.volume = sfxVolume * masterVolume;
        
        if (driftSoundSource != null)
            driftSoundSource.volume = sfxVolume * masterVolume * 0.9f;
        
        if (itemSoundSource != null)
            itemSoundSource.volume = sfxVolume * masterVolume;
        
        if (uiSoundSource != null)
            uiSoundSource.volume = sfxVolume * masterVolume * 0.8f;
    }

    /// <summary>
    /// 게임 재시작 시 모든 사운드 초기화
    /// </summary>
    public void ResetAllSounds()
    {
        Debug.Log("🔄 모든 사운드 초기화 시작!");
        
        // 모든 오디오 소스 정지
        if (backgroundMusicSource != null)
            backgroundMusicSource.Stop();
        
        if (countdownSoundSource != null)
            countdownSoundSource.Stop();
        
        if (engineSoundSource != null)
            engineSoundSource.Stop();
        
        if (boosterSoundSource != null)
        {
            boosterSoundSource.Stop();
            boosterSoundSource.loop = false;
        }
        
        if (driftSoundSource != null)
        {
            driftSoundSource.Stop();
            driftSoundSource.loop = false;
        }
        
        if (itemSoundSource != null)
            itemSoundSource.Stop();
        
        if (uiSoundSource != null)
            uiSoundSource.Stop();
        
        // 모든 Invoke 취소
        CancelInvoke();
        
        // 상태 변수 초기화
        isEngineRunning = false;
        isDrifting = false;
        isBoosting = false;
        
        Debug.Log("✅ 모든 사운드 초기화 완료!");
    }

    /// <summary>
    /// 🆕 게임 종료 시 모든 사운드 완전 정지
    /// </summary>
    public void StopAllSoundsOnQuit()
    {
        Debug.Log("🚪 게임 종료를 위한 모든 사운드 정지!");
        
        // 모든 오디오 소스 완전 정지
        AudioSource[] allAudioSources = FindObjectsOfType<AudioSource>();
        foreach (AudioSource audioSource in allAudioSources)
        {
            if (audioSource != null)
            {
                audioSource.Stop();
                audioSource.clip = null;
            }
        }
        
        // 모든 Invoke 취소
        CancelInvoke();
        
        Debug.Log("✅ 모든 사운드 완전 정지 완료!");
    }
}
