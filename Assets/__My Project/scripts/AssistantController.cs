using System.Collections;
using TMPro;
using UnityEngine;

public class AssistantController : MonoBehaviour
{
    [SerializeField] Animator animator;
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip introClip;
    [SerializeField] TMP_Text subtitleText;

    [SerializeField] GameStateController gameStateController;

    public void PlayIntro()
    {
        StartCoroutine(RunIntro());
    }

    IEnumerator RunIntro()
    {
        animator.SetTrigger("Intro");

        subtitleText.text = "You are today's new sleep organizer, right? I am your assistant.";
        //audioSource.PlayOneShot(introClip);

        yield return new WaitForSeconds(3);

        subtitleText.text = "Now place your hand on the crystal ball. We are ready to enter their subconscious.";
        gameStateController.State= GameStateController.GameState.AwaitCrystalBall;
    }
}
