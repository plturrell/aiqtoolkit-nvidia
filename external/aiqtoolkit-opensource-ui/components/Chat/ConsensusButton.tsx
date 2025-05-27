import React from 'react';
import { Button } from '@/components/ui/button';
import { Brain } from 'lucide-react';
import { useRouter } from 'next/router';

interface ConsensusButtonProps {
  messageId: string;
  hasConsensus: boolean;
  consensusId?: string;
}

export const ConsensusButton: React.FC<ConsensusButtonProps> = ({
  messageId,
  hasConsensus,
  consensusId
}) => {
  const router = useRouter();

  const handleViewConsensus = () => {
    if (consensusId) {
      // Navigate to consensus panel with specific consensus ID
      router.push(`/consensus?id=${consensusId}`);
    }
  };

  if (!hasConsensus) return null;

  return (
    <Button
      variant="outline"
      size="sm"
      onClick={handleViewConsensus}
      className="mt-2"
    >
      <Brain className="w-4 h-4 mr-2" />
      View Neural Consensus Process
    </Button>
  );
};

export default ConsensusButton;