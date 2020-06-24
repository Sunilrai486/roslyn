﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;

namespace Microsoft.CodeAnalysis.Editor.InlineParamNameHints
{
    /// <summary>
    /// The purpose of this tagger is to convert the InlineParamNameHintsDataTags to
    /// the InlineParamNameHintsTag, which actually creates the UIElement. It reacts to
    /// tags changing and updates the adornments accordingly.
    /// </summary>
    class InlineParamNameHintsTagger : ITagger<IntraTextAdornmentTag>
    {
        private readonly ITagAggregator<InlineParamNameHintDataTag> _tagAggregator;
        private readonly ITextBuffer _buffer;

        public InlineParamNameHintsTagger(ITextBuffer buffer, ITagAggregator<InlineParamNameHintDataTag> tagAggregator)
        {
            _buffer = buffer;
            _tagAggregator = tagAggregator;
            _tagAggregator.TagsChanged += _tagAggregator_TagsChanged;
        }

        private void _tagAggregator_TagsChanged(object sender, TagsChangedEventArgs e)
        {
            var spans = e.Span.GetSpans(_buffer);
            foreach (var span in spans)
            {
                TagsChanged?.Invoke(this, new SnapshotSpanEventArgs(new SnapshotSpan(span.Snapshot, 0, span.Snapshot.Length)));
            }
        }

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

        public IEnumerable<ITagSpan<IntraTextAdornmentTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            var tagsList = new List<TagSpan<IntraTextAdornmentTag>>();
            var dataTags = _tagAggregator.GetTags(spans);
            foreach(var tag in dataTags)
            {
                var dataTagSpans = tag.Span.GetSpans(spans[0].Snapshot);
                var textTag = tag.Tag;
                if (dataTagSpans.Count > 0)
                {
                    var dataTagSpan = dataTagSpans[0];
                    var adornmentSpan = new SnapshotSpan(dataTagSpan.Start, 0);
                    tagsList.Add(new TagSpan<IntraTextAdornmentTag>(adornmentSpan, new InlineParamNameHintsTag(textTag.TagName)));

                }
            }
            return tagsList.AsEnumerable();
        }
    }
}
