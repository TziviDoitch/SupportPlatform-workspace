import { Typography } from 'antd';

interface PagePlaceholderProps {
  title: string;
  description?: string;
}

/** Generic empty-screen shell used until a feature is built out. */
export function PagePlaceholder({ title, description }: PagePlaceholderProps) {
  return (
    <section>
      <Typography.Title level={3}>{title}</Typography.Title>
      {description ? (
        <Typography.Paragraph type="secondary">{description}</Typography.Paragraph>
      ) : null}
    </section>
  );
}
