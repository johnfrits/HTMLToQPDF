using HtmlAgilityPack;
using HTMLQuestPDF.Extensions;
using HTMLQuestPDF.Utils;
using HTMLToQPDF.Components;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace HTMLQuestPDF.Components
{
    internal class BaseHTMLComponent : IComponent
    {
        protected readonly HTMLComponentsArgs args;
        protected readonly HtmlNode node;

        public BaseHTMLComponent(HtmlNode node, HTMLComponentsArgs args)
        {
            this.node = node;
            this.args = args;
        }

        public void Compose(IContainer container)
        {
            if (!node.HasContent() || node.Name.ToLower() == "head") return;

            container = ApplyStyles(container);

            if (node.ChildNodes.Any())
            {
                ComposeMany(container);
            }
            else
            {
                ComposeSingle(container);
            }
        }

        protected virtual IContainer ApplyStyles(IContainer container)
        {
            // Apply predefined styles based on element tag name
            container = args.ContainerStyles.TryGetValue(node.Name.ToLower(), out var style) ? style(container) : container;

            // Apply inline styles from the style attribute
            container = ApplyInlineStyles(container);

            return container;
        }

        protected virtual IContainer ApplyInlineStyles(IContainer container)
        {
            // Parse and apply text-align from inline styles
            var textAlign = StyleUtils.GetTextAlign(node);
            if (!string.IsNullOrEmpty(textAlign))
            {
                container = textAlign.ToLower() switch
                {
                    "left" => container.AlignLeft(),
                    "center" => container.AlignCenter(),
                    "right" => container.AlignRight(),
                    "justify" => container.AlignLeft(), // QuestPDF doesn't have direct justify support, using left
                    _ => container
                };
            }

            return container;
        }

        protected virtual void ComposeSingle(IContainer container)
        {
        }

        protected virtual void ComposeMany(IContainer container)
        {
            container.Column(col =>
            {
                var buffer = new List<HtmlNode>();
                foreach (var item in node.ChildNodes)
                {
                    if (item.IsBlockNode() || item.HasBlockElement())
                    {
                        ComposeMany(col, buffer);
                        buffer.Clear();

                        col.Item().Component(item.GetComponent(args));
                    }
                    else
                    {
                        buffer.Add(item);
                    }
                }
                ComposeMany(col, buffer);
            });
        }

        private void ComposeMany(ColumnDescriptor col, List<HtmlNode> nodes)
        {
            if (nodes.Count == 1)
            {
                col.Item().Component(nodes.First().GetComponent(args));
            }
            else if (nodes.Count > 0)
            {
                col.Item().Component(new ParagraphComponent(nodes, args));
            }
        }
    }
}