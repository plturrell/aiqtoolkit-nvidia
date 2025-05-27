import { IconPlus } from '@tabler/icons-react';
import { FC } from 'react';

import { Conversation } from '@/types/chat';

interface Props {
  selectedConversation: Conversation;
  onNewConversation: () => void;
}

export const Navbar: FC<Props> = ({
  selectedConversation,
  onNewConversation,
}) => {
  return (
    <nav className="flex w-full justify-between bg-scb-blue py-3 px-4 shadow-sm">
      <div className="mr-4 flex items-center">
        <img src="/sc-logo.svg" alt="Standard Chartered" className="h-6 w-auto" />
      </div>

      <div className="max-w-[240px] overflow-hidden text-ellipsis whitespace-nowrap text-white font-medium">
        {selectedConversation.name}
      </div>

      <IconPlus
        className="cursor-pointer text-white hover:text-scb-light-blue mr-8"
        onClick={onNewConversation}
      />
    </nav>
  );
};
