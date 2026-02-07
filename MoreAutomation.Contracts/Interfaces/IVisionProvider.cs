using MoreAutomation.Contracts.Models;
using OpenCvSharp;
using System;

namespace MoreAutomation.Contracts.Interfaces
{
    public interface IVisionProvider
    {
        MatchResult FindImage(Mat screenMat, string templatePath, double threshold = 0.8);
        MatchResult FindText(Mat screenMat, string targetText);
    }
}