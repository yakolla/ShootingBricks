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
	void Start ()
	{
		interstitial = new InterstitialAd(adUnitId);
		interstitial.AdLoaded += HandleInterstitialLoaded;
		interstitial.AdLoaded += HandleInterstitialLoaded;
		interstitial.AdFailedToLoad += HandleInterstitialFailedToLoad;
		interstitial.AdOpened += HandleInterstitialOpened;
		interstitial.AdClosing += HandleInterstitialClosing;
		interstitial.AdClosed += HandleInterstitialClosed;
		interstitial.AdLeftApplication += HandleInterstitialLeftApplication;

		//bannerView = new BannerView(APPID, AdSize.Banner, AdPosition.Top);
		// Create an empty ad request.
		AdRequest request = new AdRequest.Builder()
			.AddTestDevice(AdRequest.TestDeviceSimulator)
				.AddKeyword("game")
				.Build();
		// Load the Ads with the request.
		//bannerView.LoadAd(request);		
		
		interstitial.LoadAd(request);
	}
	public void HandleInterstitialLoaded(object sender, System.EventArgs args)
	{
		interstitial.Show();  
		print("HandleInterstitialLoaded event received.");
	}
	
	public void HandleInterstitialFailedToLoad(object sender, AdFailedToLoadEventArgs args)
	{
		print("HandleInterstitialFailedToLoad event received with message: " + args.Message);
	}
	
	public void HandleInterstitialOpened(object sender, EventArgs args)
	{
		print("HandleInterstitialOpened event received");
	}
	
	void HandleInterstitialClosing(object sender, EventArgs args)
	{
		print("HandleInterstitialClosing event received");
	}
	
	public void HandleInterstitialClosed(object sender, EventArgs args)
	{
		print("HandleInterstitialClosed event received");
	}
	
	public void HandleInterstitialLeftApplication(object sender, EventArgs args)
	{
		print("HandleInterstitialLeftApplication event received");
	}
	// Update is called once per frame
	void Update ()
	{

	}
}

