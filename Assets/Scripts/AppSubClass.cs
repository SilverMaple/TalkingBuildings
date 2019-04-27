using UnityEngine;
using System.Collections;
using LeanCloud;
using System;
using System.Collections.Generic;

//子类化
// 地点类
[AVClassName("Place")]
public class Place : AVObject
{
    [AVFieldName("placeName")]
    public string PlaceName
    {
        get { return GetProperty<string>("PlaceName"); }
        set { SetProperty<string>(value, "PlaceName"); }
    }

    [AVFieldName("avgRating")]
    public double AvgRating
    {
        get { return GetProperty<double>("AvgRating"); }
        set { SetProperty<double>(value, "AvgRating"); }
    }

    [AVFieldName("ratingSum")]
    public int RatingSum
    {
        get { return GetProperty<int>("RatingSum"); }
        set { SetProperty<int>(value, "RatingSum"); }
    }

    [AVFieldName("tagList")]
    public List<string> TagList
    {
        get { return GetProperty<List<string>>("TagList"); }
        set { SetProperty<List<string>>(value, "TagList"); }
    }
}

// 评论类
[AVClassName("Comment")]
public class Comment : AVObject
{
    [AVFieldName("content")]
    public string Content
    {
        get { return GetProperty<string>("Content"); }
        set { SetProperty<string>(value, "Content"); }
    }

    [AVFieldName("place")]
    public Place Place
    {
        get { return GetProperty<Place>("Place"); }
        set { SetProperty<Place>(value, "Place"); }
    }

    [AVFieldName("user")]
    public AVUser User
    {
        get { return GetProperty<AVUser>("User"); }
        set { SetProperty<AVUser>(value, "User"); }
    }

    [AVFieldName("rating")]
    public double Rating
    {
        get { return GetProperty<double>("Rating"); }
        set { SetProperty<double>(value, "Rating"); }
    }

    [AVFieldName("tagList")]
    public List<string> TagList
    {
        get { return GetProperty<List<string>>("TagList"); }
        set { SetProperty<List<string>>(value, "TagList"); }
    }
}