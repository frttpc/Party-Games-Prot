using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("General")]
    [SerializeField] [Range(0, 1)] private float musicVolume;
    [SerializeField] [Range(0, 1)] private float soundVolume;
    [SerializeField] private float walkSoundDelay = 0.1f;

    [Header("Music")]
    [SerializeField] private AudioSource musicPlayer;
    [SerializeField] private List<AudioClip> backgroundSongs;

    [Header("Player Sounds")]
    [SerializeField] private List<AudioClip> footstepSounds;
    [SerializeField] private List<AudioClip> pushSounds;
    [SerializeField] private List<AudioClip> jumpSounds;
    [SerializeField] private List<AudioClip> dashSounds;
    [SerializeField] private List<AudioClip> attackSounds;

    [Space]
    [SerializeField] private AudioClip respawnSound;

    private Dictionary<PlayerState, List<AudioClip>> soundsDictionary = new();

    private float walkSoundDelayCounter;

    public static AudioManager Instance;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        soundsDictionary.Add(PlayerState.Walk, footstepSounds);
        soundsDictionary.Add(PlayerState.Push, pushSounds);
        soundsDictionary.Add(PlayerState.Jump, jumpSounds);
        soundsDictionary.Add(PlayerState.Dash, dashSounds);
        soundsDictionary.Add(PlayerState.Attack, attackSounds);

        walkSoundDelayCounter = 0;
    }

    void Update()
    {
        if (GameManager.Instance.isStarted && !musicPlayer.isPlaying)
            PlayBackgroundMusic();

        walkSoundDelayCounter -= Time.deltaTime;

        musicPlayer.volume = musicVolume * 0.5f;
    }

    private void PlayBackgroundMusic()
    {
        musicPlayer.PlayOneShot(backgroundSongs[Random.Range(0, backgroundSongs.Count - 1)]);
    }

    public void PlaySound(PlayerState state, AudioSource source)
    {
        source.volume = soundVolume * 0.5f;

        if(state == PlayerState.Walk)
        {
            if (walkSoundDelayCounter > 0)
                return;
            else
                walkSoundDelayCounter = walkSoundDelay;
            source.volume *= 0.5f;
        }

        source.PlayOneShot(soundsDictionary[state][Random.Range(0, soundsDictionary[state].Count - 1)]);
    }

    public void PlayRespawnSound(AudioSource source)
    {
        source.PlayOneShot(respawnSound);
    }
}
