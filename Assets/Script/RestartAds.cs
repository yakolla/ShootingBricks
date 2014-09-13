using UnityEngine;
using System.Collections;
using GoogleMobileAds.Api;  // 반드시 추가
using System;

public class RestartAds : MonoBehaviour
{
	InterstitialAd interstitial;
	#if UNITY_EDITOR
	string adUnitId = "unused";
	#elif UNITY_ANDROID
	string adUnitId = "ca-app-pub-8449608955539666/3023965631";
	#elif UNITY_IPHONE
	string adUnitId = "INSERT_IOS_INTERSTITIAL_AD_UNIT_ID_HERE";
	#else
	string adUnitId = "unexpected_platform";
	#endif
	// Use this for initialization
	public event EventHandler<EventArgs> OnClosedAds = delegate {};

	void Awake()
	{
		interstitial = new InterstitialAd(adUnitId);
		interstitial.AdLoaded += HandleInterstitialLoaded;
		interstitial.AdFailedToLoad += HandleInterstitialFailedToLoad;
		interstitial.AdOpened += HandleInterstitialOpened;
		interstitial.AdClosing += HandleInterstitialClosing;
		interstitial.AdClosed += HandleInterstitialClosed;
		interstitial.AdLeftApplication += HandleInterstitialLeftApplication;
	}

	void Start ()
	{

	}

	public void show()
	{		
		//bannerView = new BannerView(APPID, AdSize.Banner, AdPosition.Top);
		// Create an empty ad request.
		AdRequest request = new AdRequest.Builder()
			.AddKeyword("game")
				.Build();
		// Load the Ads with the request.
		//bannerView.LoadAd(request);		
		
		interstitial.LoadAd(request);
	}

	public void HandleInterstitialLoaded(object sender, System.EventArgs args)
	{
		interstitial.Show();  
		Debug.Log("HandleInterstitialLoaded event received.");
	}
	
	public void HandleInterstitialFailedToLoad(object sender, AdFailedToLoadEventArgs args)
	{
		Debug.Log("HandleInterstitialFailedToLoad event received with message: " + args.Message);
		OnClosedAds(this, EventArgs.Empty);
	}
	
	public void HandleInterstitialOpened(object sender, EventArgs args)
	{
		Debug.Log("HandleInterstitialOpened event received");
	}
	
	void HandleInterstitialClosing(object sender, EventArgs args)
	{
		Debug.Log("HandleInterstitialClosing event received");
	}
	
	public void HandleInterstitialClosed(object sender, EventArgs args)
	{
		Debug.Log("HandleInterstitialClosed event received");
		OnClosedAds(this, EventArgs.Empty);
	}
	
	public void HandleInterstitialLeftApplication(object sender, EventArgs args)
	{
		Debug.Log("HandleInterstitialLeftApplication event received");
	}
	// Update is called once per frame
	void Update ()
	{

	}
}

