export type CreateDatabaseModalState = {
	isOpen: boolean;
	open: () => void;
	close: () => void;
	toggle: () => void;
};

export type DeleteDatabaseModalState = {
	isOpen: boolean;
	databaseId: string | null;
	databaseName: string | null;
	open: (databaseId: string, databaseName: string) => void;
	close: () => void;
	toggle: () => void;
};
