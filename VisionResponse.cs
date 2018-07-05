using System.Collections.Generic;

namespace MtgLib
{
    /// <summary>
    /// What comes back from GoogleVision API call
    /// </summary>
    public class VisionResponse
    {
        public class FullTextAnnotation
        {
            public List<object> pages;              // don't care
            public string text;
        }

        public class Response
        {
            public List<object> textAnnotations;    // don't care
            public FullTextAnnotation FullTextAnnotation;
        }

        public List<Response> responses;
    }
}
