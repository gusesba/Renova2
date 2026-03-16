type FeedbackBannerProps = {
  message: string;
};

export function FeedbackBanner({ message }: FeedbackBannerProps) {
  return <div className="ui-banner">{message}</div>;
}
